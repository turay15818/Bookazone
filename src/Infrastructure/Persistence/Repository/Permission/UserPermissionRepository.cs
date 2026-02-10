using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Repository.Permission;
using Bookazone.Domain.Entities.Permissions;
using Bookazone.Infrastructure.Persistence.DbContext;


namespace Bookazone.Infrastructure.Persistence.Repository.Permission;


public class UserPermissionRepository(BookazoneDbContext BookazoneDbContext) : IUserPermissionRepository
{
    public List<UserPermission> All()
    {
        return (BookazoneDbContext.UserPermissions ?? throw new Except(ErrorHttp.DbQueryRunFailed)).Where(a => a.Deleted == false).ToListAsync().Result;
    }
    
    
    public async Task<bool> SyncUserPermissionsAsync(Guid userId, List<Guid> permissionIds, string? updatedBy = null)
    {
        var existing = await BookazoneDbContext.UserPermissions
            .Where(up => up.FkUserId == userId && up.Deleted != true)
            .ToListAsync();
        var toRemove = existing.Where(e => !permissionIds.Contains(e.FkPermissionId)).ToList();
        // Soft-delete removals
        foreach (var e in toRemove)
        {
            e.Active = false;
            e.Deleted = true;
            e.DateDeleted = DateTime.UtcNow;
            e.UpdatedBy = updatedBy;
            e.DateUpdated = DateTime.UtcNow;
        }
        var existingIds = existing.Select(e => e.FkPermissionId).ToList();
        var toAdd = permissionIds.Where(pid => !existingIds.Contains(pid)).ToList();
        foreach (var permId in toAdd)
        {
            BookazoneDbContext.UserPermissions.Add(new UserPermission
            {
                FkUserId = userId,
                FkPermissionId = permId,
                CreatedBy = updatedBy,
                DateCreated = DateTime.UtcNow,
                Active = true,
                Deleted = false
            });
        }
        return await BookazoneDbContext.SaveChangesAsync() > 0;
    }

    public async Task<bool> SyncUserPermissionsByNamesAsync(Guid userId, List<string>? permissionNames, string? updatedBy = null)
    {
        if (BookazoneDbContext.UserPermissions == null || BookazoneDbContext.Permissions == null)
            throw new Except(ErrorHttp.DbQueryRunFailed);
        // load existing mappings including deleted, with permission
        var existing = await BookazoneDbContext.UserPermissions
            .Include(up => up.FkPermission)
            .Where(up => up.FkUserId == userId)
            .ToListAsync();

        if (permissionNames == null || permissionNames.Count == 0)
        {
            // clear all (soft delete)
            foreach (var e in existing.Where(x => x.Deleted != true))
            {
                e.Active = false;
                e.Deleted = true;
                e.DateDeleted = DateTime.UtcNow;
                e.UpdatedBy = updatedBy;
                e.DateUpdated = DateTime.UtcNow;
            }
            return await BookazoneDbContext.SaveChangesAsync() > 0;
        }

        var normalized = new HashSet<string>(permissionNames.Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n.Trim()), StringComparer.OrdinalIgnoreCase);
        // Fetch target permissions
        var targetPerms = await BookazoneDbContext.Permissions
            .Where(p => normalized.Contains(p.Name) && p.Active && !p.Deleted)
            .ToListAsync();
        var targetByName = targetPerms.ToDictionary(p => p.Name!, p => p, StringComparer.OrdinalIgnoreCase);

        // Soft-delete those not in provided list
        foreach (var e in existing.Where(x => x.Deleted != true))
        {
            var permName = e.FkPermission?.Name;
            if (string.IsNullOrWhiteSpace(permName) || !normalized.Contains(permName))
            {
                e.Active = false;
                e.Deleted = true;
                e.DateDeleted = DateTime.UtcNow;
                e.UpdatedBy = updatedBy;
                e.DateUpdated = DateTime.UtcNow;
            }
        }

        // Add or reactivate provided ones
        foreach (var name in normalized)
        {
            if (!targetByName.TryGetValue(name, out var perm))
                continue; // skip unknown names

            var existingMap = existing.FirstOrDefault(x => x.FkPermissionId == perm.Id);
            if (existingMap == null)
            {
                BookazoneDbContext.UserPermissions.Add(new UserPermission
                {
                    FkUserId = userId,
                    FkPermissionId = perm.Id,
                    CreatedBy = updatedBy,
                    DateCreated = DateTime.UtcNow,
                    Active = true,
                    Deleted = false
                });
            }
            else if (existingMap.Deleted)
            {
                existingMap.Deleted = false;
                existingMap.Active = true;
                existingMap.DateUpdated = DateTime.UtcNow;
                existingMap.UpdatedBy = updatedBy;
            }
            else if (!existingMap.Active)
            {
                existingMap.Active = true;
                existingMap.DateUpdated = DateTime.UtcNow;
                existingMap.UpdatedBy = updatedBy;
            }
        }

        return await BookazoneDbContext.SaveChangesAsync() > 0;
    }

    public List<UserPermission> FindByUserId(Guid? userId)
    {
        if (BookazoneDbContext.UserPermissions == null) throw new Except(ErrorHttp.DbQueryRunFailed);
        return BookazoneDbContext.UserPermissions.Where(a => a.FkUserId == userId && a.Deleted != true).ToListAsync().Result;
    }

    public UserPermission? Find(Guid? id)
    {
        if (BookazoneDbContext.UserPermissions == null) throw new Except(ErrorHttp.DbQueryRunFailed);
        return BookazoneDbContext.UserPermissions.FirstOrDefault(a => a.Id == id && a.Deleted != true);
    }
    
    public UserPermission? Create(UserPermission entity)
    {
        entity.Active = true;
        entity.Deleted = false;
        entity.DateCreated = DateTime.UtcNow;
        entity.CreatedBy = "USER";
        
        if (BookazoneDbContext.UserPermissions == null) throw new Except(ErrorHttp.DbCreateError);
        BookazoneDbContext.UserPermissions.Add(entity);
        var result = BookazoneDbContext.SaveChangesAsync().Result;
        return result > 0 ? entity : null;
    }

    public UserPermission? Update(UserPermission entity)
    {
        entity.DateUpdated = DateTime.UtcNow;

        if (BookazoneDbContext.UserPermissions == null) throw new Except(ErrorHttp.DbCreateError);
        BookazoneDbContext.UserPermissions.Update(entity);
        var result = BookazoneDbContext.SaveChangesAsync().Result;
        return result > 0 ? entity : null;
    }

    public bool Delete(Guid? id)
    {
        var entity = Find(id);
        if (id == null || entity == null) throw new Except(ErrorHttp.NotFound);
        entity.Active = false;
        entity.Deleted = true;
        entity.DateDeleted = DateTime.UtcNow;
        if (BookazoneDbContext.UserPermissions == null) throw new Except(ErrorHttp.DbCreateError);
        BookazoneDbContext.UserPermissions.Update(entity);
        var result = BookazoneDbContext.SaveChangesAsync().Result; return result > 0;
    }

    public bool Status(Guid? id, bool active)
    {
        var entity = Find(id);
        if (id == null || entity == null) throw new Except(ErrorHttp.NotFound);
        entity.Active = active;
        entity.DateUpdated = DateTime.UtcNow;
        if (BookazoneDbContext.UserPermissions == null) throw new Except(ErrorHttp.DbCreateError);
        BookazoneDbContext.UserPermissions.Update(entity);
        var result = BookazoneDbContext.SaveChangesAsync().Result; return result > 0;
    }
}