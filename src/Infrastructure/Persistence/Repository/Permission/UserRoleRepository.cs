using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Repository.Permission;
using Bookazone.Domain.Entities.Permissions;
using Bookazone.Infrastructure.Persistence.DbContext;


namespace Bookazone.Infrastructure.Persistence.Repository.Permission;

public class UserRoleRepository(BookazoneDbContext BookazoneDbContext, ILogger<UserRoleRepository> logger) : IUserRoleRepository
{
    #region CRUD

    public async Task<List<UserRole>> AllAsync()
    {
        if (BookazoneDbContext.UserRoles == null)
            throw new Except(ErrorHttp.DbQueryRunFailed);

        return await BookazoneDbContext
            .UserRoles.Include(ur => ur.FkUser)
            .Include(ur => ur.FkRole)
            .Where(ur => ur.Deleted != true)
            .ToListAsync();
    }

    public List<UserRole> All()
    {
        if (BookazoneDbContext.UserRoles == null)
            throw new Except(ErrorHttp.DbQueryRunFailed);

        return BookazoneDbContext
            .UserRoles.Include(ur => ur.FkUser)
            .Include(ur => ur.FkRole)
            .Where(ur => ur.Deleted != true)
            .ToList();
    }

    public async Task<UserRole?> FindAsync(Guid? id)
    {
        if (BookazoneDbContext.UserRoles == null)
            throw new Except(ErrorHttp.DbQueryRunFailed);

        return await BookazoneDbContext
            .UserRoles.Include(ur => ur.FkUser)
            .Include(ur => ur.FkRole)
            .FirstOrDefaultAsync(ur => ur.Id == id && ur.Deleted != true);
    }

    public List<UserRole> FindByUserId(Guid? userId)
    {
        if (BookazoneDbContext.UserRoles == null) throw new Except(ErrorHttp.DbQueryRunFailed);
        return BookazoneDbContext.UserRoles.Where(a => a.FkUser.Id == userId && a.Deleted != true).ToListAsync().Result;
    }
    public UserRole? Find(Guid? id)
    {
        if (BookazoneDbContext.UserRoles == null)
            throw new Except(ErrorHttp.DbQueryRunFailed);

        return BookazoneDbContext
            .UserRoles.Include(ur => ur.FkUser)
            .Include(ur => ur.FkRole)
            .FirstOrDefault(ur => ur.Id == id && ur.Deleted != true);
    }

    public async Task<UserRole?> CreateAsync(UserRole entity)
    {
        entity.Active = true;
        entity.Deleted = false;
        entity.DateCreated = DateTime.UtcNow;

        BookazoneDbContext.UserRoles.Add(entity);
        var result = await BookazoneDbContext.SaveChangesAsync();
        return result > 0 ? entity : null;
    }

    public UserRole? Create(UserRole entity)
    {
        entity.Active = true;
        entity.Deleted = false;
        entity.DateCreated = DateTime.UtcNow;

        BookazoneDbContext.UserRoles.Add(entity);
        var result = BookazoneDbContext.SaveChanges();
        return result > 0 ? entity : null;
    }

    public async Task<UserRole?> UpdateAsync(UserRole entity)
    {
        entity.DateUpdated = DateTime.UtcNow;
        BookazoneDbContext.UserRoles.Update(entity);
        var result = await BookazoneDbContext.SaveChangesAsync();
        return result > 0 ? entity : null;
    }

    public UserRole? Update(UserRole entity)
    {
        entity.DateUpdated = DateTime.UtcNow;
        BookazoneDbContext.UserRoles.Update(entity);
        var result = BookazoneDbContext.SaveChanges();
        return result > 0 ? entity : null;
    }

    public async Task<bool> DeleteAsync(Guid? id)
    {
        var entity = await FindAsync(id);
        if (entity == null)
            throw new Except(ErrorHttp.NotFound);

        entity.Active = false;
        entity.Deleted = true;
        entity.DateDeleted = DateTime.UtcNow;
        BookazoneDbContext.UserRoles.Update(entity);

        var result = await BookazoneDbContext.SaveChangesAsync();
        return result > 0;
    }

    public bool Delete(Guid? id)
    {
        var entity = Find(id);
        if (entity == null)
            throw new Except(ErrorHttp.NotFound);

        entity.Active = false;
        entity.Deleted = true;
        entity.DateDeleted = DateTime.UtcNow;
        BookazoneDbContext.UserRoles.Update(entity);

        var result = BookazoneDbContext.SaveChanges();
        return result > 0;
    }

    public async Task<bool> StatusAsync(Guid? id, bool active)
    {
        var entity = await FindAsync(id);
        if (entity == null)
            throw new Except(ErrorHttp.NotFound);

        entity.Active = active;
        entity.DateUpdated = DateTime.UtcNow;
        BookazoneDbContext.UserRoles.Update(entity);
        var result = await BookazoneDbContext.SaveChangesAsync();
        return result > 0;
    }

    public bool Status(Guid? id, bool active)
    {
        var entity = Find(id);
        if (entity == null)
            throw new Except(ErrorHttp.NotFound);

        entity.Active = active;
        entity.DateUpdated = DateTime.UtcNow;
        BookazoneDbContext.UserRoles.Update(entity);
        var result = BookazoneDbContext.SaveChanges();
        return result > 0;
    }

    #endregion

    #region Relationship Management

    public async Task<bool> AssignRolesToUserAsync(
        Guid userId,
        List<Guid> roleIds,
        string? updatedBy = null
    )
    {
        if (roleIds == null || roleIds.Count == 0)
            return false;

        var existingRoles = await BookazoneDbContext
            .UserRoles.Where(ur => ur.FkUserId == userId && ur.Deleted != true)
            .ToListAsync();

        // Remove duplicates (keep only new ones)
        var newRoleIds = roleIds.Except(existingRoles.Select(er => er.FkRoleId)).ToList();

        foreach (var roleId in newRoleIds)
        {
            BookazoneDbContext.UserRoles.Add(
                new UserRole
                {
                    FkUserId = userId,
                    FkRoleId = roleId,
                    Active = true,
                    Deleted = false,
                    CreatedBy = updatedBy,
                    DateCreated = DateTime.UtcNow,
                }
            );
        }

        var result = await BookazoneDbContext.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> RemoveRolesFromUserAsync(Guid userId, List<Guid>? roleIds = null)
    {
        var query = BookazoneDbContext.UserRoles.Where(ur => ur.FkUserId == userId && ur.Deleted != true);

        if (roleIds != null && roleIds.Count > 0)
            query = query.Where(ur => roleIds.Contains(ur.FkRoleId));

        var rolesToRemove = await query.ToListAsync();

        if (!rolesToRemove.Any())
            return false;

        foreach (var userRole in rolesToRemove)
        {
            userRole.Active = false;
            userRole.Deleted = true;
            userRole.DateDeleted = DateTime.UtcNow;
        }

        BookazoneDbContext.UserRoles.UpdateRange(rolesToRemove);
        var result = await BookazoneDbContext.SaveChangesAsync();
        return result > 0;
    }

    

    
    public async Task<List<Role>> GetRolesByUserAsync(Guid userId)
    {
        var roles = await BookazoneDbContext
            .UserRoles
            .Where(ur => ur.FkUserId == userId && ur.Deleted != true && ur.FkRole != null)
            .Select(ur => ur.FkRole!)
            .ToListAsync();

        return roles;
    }

    public async Task<bool> UserHasRoleAsync(Guid userId, string roleName)
    {
        return await BookazoneDbContext
            .UserRoles.Include(ur => ur.FkRole)
            .AnyAsync(ur =>
                ur.FkUserId == userId
                && ur.FkRole != null
                && ur.FkRole.Name == roleName
                && ur.Deleted != true
            );
    }

    #endregion
}
