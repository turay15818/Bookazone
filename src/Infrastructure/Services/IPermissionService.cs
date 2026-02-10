using Microsoft.EntityFrameworkCore;
using Bookazone.Infrastructure.Persistence;
using Bookazone.Infrastructure.Persistence.DbContext;

namespace Bookazone.Infrastructure.Services;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid userId, string permissionSlug);
    Task<List<string>> GetAllPermissionsForUserAsync(Guid userId);
    Task<List<string>> GetPermissionsFromRoleAsync(Guid roleId);
}


public class PermissionService(BookazoneDbContext dbContext, ILogger<PermissionService> logger)
    : IPermissionService
{
    /// <summary>
        /// Check if a user has a given permission (direct or via role)
        /// </summary>
        public async Task<bool> HasPermissionAsync(Guid userId, string permissionSlug)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(permissionSlug))
                    return false;

                // Normalize slug (just in case)
                var slug = permissionSlug.Trim().ToUpper();

                // Check direct user permissions
                var hasDirectPermission = await dbContext.UserPermissions
                    .Include(up => up.FkPermission)
                    .AnyAsync(up => up.FkUserId == userId && 
                                    up.FkPermission.Slug == slug &&
                                    up.FkPermission.Active && !up.FkPermission.Deleted);

                if (hasDirectPermission)
                    return true;

                // Check role-based permissions
                var hasRolePermission = await dbContext.UserRoles
                    .Include(ur => ur.FkRole)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.FkPermission)
                    .AnyAsync(ur =>
                        ur.FkUserId == userId &&
                        ur.FkRole!.RolePermissions.Any(rp =>
                            rp.FkPermission!.Slug == slug &&
                            rp.FkPermission.Active &&
                            !rp.FkPermission.Deleted)
                    );

                return hasRolePermission;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking permission {PermissionSlug} for user {UserId}", permissionSlug, userId);
                return false;
            }
        }

        /// <summary>
        /// Returns all permission slugs available to a user
        /// </summary>
        public async Task<List<string>> GetAllPermissionsForUserAsync(Guid userId)
        {
            try
            {
                // Direct permissions
                var userPerms = await dbContext.UserPermissions
                    .Include(up => up.FkPermission)
                    .Where(up => up.FkUserId == userId && up.FkPermission.Active && !up.FkPermission.Deleted)
                    .Select(up => up.FkPermission.Slug)
                    .ToListAsync();

                // Role-based permissions
                var rolePerms = await dbContext.UserRoles
                    .Include(ur => ur.FkRole)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.FkPermission)
                    .Where(ur => ur.FkUserId == userId)
                    .SelectMany(ur => ur.FkRole!.RolePermissions.Select(rp => rp.FkPermission!.Slug))
                    .ToListAsync();

                // Merge + Distinct
                var all = userPerms.Concat(rolePerms)
                                   .Distinct(StringComparer.OrdinalIgnoreCase)
                                   .ToList();

                return all;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching permissions for user {UserId}", userId);
                return new List<string>();
            }
        }

        /// <summary>
        /// Returns all permission slugs from a specific role
        /// </summary>
        public async Task<List<string>> GetPermissionsFromRoleAsync(Guid roleId)
        {
            try
            {
                var rolePermissions = await dbContext.RolePermissions
                    .Include(rp => rp.FkPermission)
                    .Where(rp => rp.FkRoleId == roleId && rp.FkPermission != null && rp.FkPermission.Active && !rp.FkPermission.Deleted)
                    .Select(rp => rp.FkPermission!.Slug)
                    .ToListAsync();
                return rolePermissions;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching permissions for role {RoleId}", roleId);
                return new List<string>();
            }
        }
    }