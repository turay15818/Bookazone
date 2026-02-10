using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Repository.Permission;
using Bookazone.Domain.Entities.Permissions;
using Bookazone.Infrastructure.Persistence.DbContext;


namespace Bookazone.Infrastructure.Persistence.Repository.Permission;

public class RoleRepository(
    BookazoneDbContext dbContext,
    ILogger<RoleRepository> logger
) : IRoleRepository
{
    #region Basic CRUD

    public async Task<List<Role>> AllAsync()
    {
        if (dbContext.Roles == null) throw new Except(ErrorHttp.DbQueryRunFailed);

        return await dbContext.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.FkPermission)
            .Where(r => r.Deleted != true)
            .ToListAsync();
    }
    

    public async Task<Role?> FindAsync(Guid? id)
    {
        if (dbContext.Roles == null) throw new Except(ErrorHttp.DbQueryRunFailed);

        return await dbContext.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.FkPermission)
            .FirstOrDefaultAsync(r => r.Id == id && r.Deleted != true);
    }
    
    public async Task<Role?> FindByNameAsync(string? name)
    {
        if (dbContext.Roles == null) throw new Except(ErrorHttp.DbQueryRunFailed);

        return await dbContext.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.FkPermission)
            .FirstOrDefaultAsync(r => r.Name == name && r.Deleted != true);
    }
    
    public async Task<Role?> CreateAsync(Role entity)
    {
        var existing = await dbContext.Roles
            .FirstOrDefaultAsync(r => r.Name == entity.Name && r.Deleted != true);

        if (existing != null)
            throw new Except(ErrorHttp.AlreadyExists);

        entity.Active = true;
        entity.Deleted = false;
        entity.DateCreated = DateTime.UtcNow;

        dbContext.Roles.Add(entity);
        var result = await dbContext.SaveChangesAsync();
        return result > 0 ? entity : null;
    }
    
    public async Task<Role?> UpdateAsync(Role entity)
    {
        entity.DateUpdated = DateTime.UtcNow;
        dbContext.Roles.Update(entity);
        var result = await dbContext.SaveChangesAsync();
        return result > 0 ? entity : null;
    }
    
    public async Task<bool> DeleteAsync(Guid? id)
    {
        var entity = await FindAsync(id);
        if (id == null || entity == null)
            throw new Except(ErrorHttp.NotFound);

        entity.Active = false;
        entity.Deleted = true;
        entity.DateDeleted = DateTime.UtcNow;
        dbContext.Roles.Update(entity);
        var result = await dbContext.SaveChangesAsync();
        return result > 0;
    }
    public async Task<bool> StatusAsync(Guid? id, bool active)
    {
        var entity = await FindAsync(id);
        if (id == null || entity == null)
            throw new Except(ErrorHttp.NotFound);

        entity.Active = active;
        entity.DateUpdated = DateTime.UtcNow;
        dbContext.Roles.Update(entity);
        var result = await dbContext.SaveChangesAsync();
        return result > 0;
    }

    #endregion

    #region Role-Permission Management

    public async Task<bool> AssignPermissionsAsync(Guid roleId, List<Guid> permissionIds, string? updatedBy = null)
    {
        var role = await dbContext.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == roleId && r.Deleted != true);

        if (role == null)
            throw new Except(ErrorHttp.NotFound);

        var existingPermIds = role.RolePermissions.Select(rp => rp.FkPermissionId).ToList();
        var toAdd = permissionIds.Except(existingPermIds).ToList();
        var toRemove = existingPermIds.Except(permissionIds).ToList();

        foreach (var permId in toAdd)
        {
            dbContext.RolePermissions.Add(new RolePermission
            {
                FkRoleId = roleId,
                FkPermissionId = permId,
                CreatedBy = updatedBy,
                DateCreated = DateTime.UtcNow
            });
        }
        
        if (toRemove.Any())
        {
            var removeList = role.RolePermissions.Where(rp => toRemove.Contains(rp.FkPermissionId)).ToList();
            dbContext.RolePermissions.RemoveRange(removeList);
        }
        
        role.DateUpdated = DateTime.UtcNow;
        role.UpdatedBy = updatedBy;

        var result = await dbContext.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> UpdateRolePermissionsAsync(Guid roleId, List<Guid>? providedPermissions, string? updatedBy = null)
    {
        var role = await dbContext.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == roleId && r.Deleted != true);

        if (role == null)
            throw new Except(ErrorHttp.NotFound);

        var existing = role.RolePermissions.Where(rp => rp.Deleted != true).ToList();
        var existingIds = existing.Select(rp => rp.FkPermissionId).ToList();
        var newIds = providedPermissions?.Distinct().ToList() ?? new List<Guid>();

        bool changed = false;

        // ✅ Step 1: Deactivate permissions that are not in the new payload
        var toDeactivate = existing.Where(rp => !newIds.Contains(rp.FkPermissionId)).ToList();
        foreach (var rp in toDeactivate)
        {
            rp.Deleted = true;
            rp.Active = false;
            rp.DeletedBy = updatedBy;
            rp.DateDeleted = DateTime.UtcNow;
            dbContext.RolePermissions.Update(rp);
            changed = true;
        }

        // ✅ Step 2: Add new permissions not already linked
        var toAdd = newIds.Except(existingIds).ToList();
        foreach (var permId in toAdd)
        {
            dbContext.RolePermissions.Add(new RolePermission
            {
                FkRoleId = role.Id,
                FkPermissionId = permId,
                CreatedBy = updatedBy,
                Active = true,
                Deleted = false,
                DateCreated = DateTime.UtcNow
            });
            changed = true;
        }

        // ✅ Step 3: Update metadata
        if (changed)
        {
            role.UpdatedBy = updatedBy;
            role.DateUpdated = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
        }

        return changed;
    }

    public async Task<List<object>> GetAllRolesWithPermissionsAsync()
    {
        var rolesWithPermissions = await dbContext.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.FkPermission)
            .Where(r => r.Deleted != true && r.Active == true)
            .ToListAsync();

        // Filter to roles that have at least one active permission
        var result = rolesWithPermissions
            .Where(r => r.RolePermissions.Any(rp => rp.FkPermission != null && rp.FkPermission.Deleted != true && rp.Active == true))
            .Select(r => new
            {
                RoleId = r.Id,
                RoleName = r.Name,
                RoleDescription = r.Description,
                Permissions = r.RolePermissions
                    .Where(rp => rp.FkPermission != null && rp.FkPermission.Deleted != true && rp.Active == true)
                    .Select(rp => new
                    {
                        rp.FkPermission!.Id,
                        rp.FkPermission.Name,
                        rp.FkPermission.Description
                    })
                    .ToList()
            })
            .OrderBy(r => r.RoleName)
            .ToList<object>();

        return result;
    }

    public async Task<List<Domain.Entities.Permissions.Permission>> GetPermissionsByRoleAsync(Guid roleId)
    {
        var role = await dbContext.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.FkPermission)
            .FirstOrDefaultAsync(r => r.Id == roleId && r.Deleted != true);

        if (role == null)
            throw new Except(ErrorHttp.NotFound);

        return role.RolePermissions
            .Where(rp => rp.FkPermission != null && rp.FkPermission.Deleted != true)
            .Select(rp => rp.FkPermission!)
            .ToList();
    }

    public async Task<bool> ClearPermissionsAsync(Guid roleId)
    {
        var role = await dbContext.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == roleId && r.Deleted != true);
        if (role == null)
            throw new Except(ErrorHttp.NotFound);
        dbContext.RolePermissions.RemoveRange(role.RolePermissions);
        var result = await dbContext.SaveChangesAsync();
        return result > 0;
    }

    #endregion
}