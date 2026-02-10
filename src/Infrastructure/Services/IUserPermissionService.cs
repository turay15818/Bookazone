using Microsoft.EntityFrameworkCore;
using Bookazone.Domain.Entities.Permissions;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Domain.Entities.Tenant;
using Bookazone.Infrastructure.Persistence;

using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Repository.Permission;
using Bookazone.Infrastructure.Persistence.DbContext;

namespace Bookazone.Infrastructure.Services;

public interface IUserPermissionService
{
    // Main methods
    Task AssignRoleWithPermissionsToUserAsync(Users user, string roleName, string createdBy);
    Task AssignMultipleRolesWithPermissionsToUserAsync(Users user, List<string> roleNames, string createdBy);
    Task SyncUserPermissionsFromRolesAsync(Users user, string updatedBy);
    Task RemoveAllUserPermissionsAsync(Users user);
    Task<List<Permission>> GetUserEffectivePermissionsAsync(Guid userId);
    Task<List<string>> GetUserPermissionNamesAsync(Guid userId);
    
    // Convenience/Helper methods (for backward compatibility)
    Task SetupTenantAdminAsync(Users user, Tenants tenant, string createdBy);
    Task SetupDefaultUserAsync(Users user, Tenants? tenant, string createdBy);
    Task SetupUserWithRolesAsync(Users user, List<string> roleNames, Tenants? tenant, string createdBy);
    Task InitializeDefaultTenantPermissionsAsync(Tenants tenant, string createdBy);
    Task SetupCustomerAsync(Users user, string createdBy);

}

public class UserPermissionService(
    BookazoneDbContext context,
    ILogger<UserPermissionService> logger,
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository,
    IUserRoleRepository userRoleRepository,
    IUserPermissionRepository userPermissionRepository)
    : IUserPermissionService
{
    private readonly IPermissionRepository _permissionRepository = permissionRepository;
    private readonly IUserRoleRepository _userRoleRepository = userRoleRepository;
    private readonly IUserPermissionRepository _userPermissionRepository = userPermissionRepository;

    #region Main Methods

    /// <summary>
    /// Assigns a role to a user and all its permissions
    /// </summary>
    public async Task AssignRoleWithPermissionsToUserAsync(Users user, string roleName, string createdBy)
    {
        logger.LogInformation($"Assigning role {roleName} with permissions to user {user.Id}");

        // Step 1: Find the role in the database
        var role = await roleRepository.FindByNameAsync(roleName);
        if (role == null)
        {
            throw new Exception($"Role '{roleName}' not found in database");
        }

        // Step 2: Assign role to user via UserRole
        var existingUserRole = await context.UserRoles
            .FirstOrDefaultAsync(ur => ur.FkUserId == user.Id && ur.FkRoleId == role.Id && !ur.Deleted);

        if (existingUserRole == null)
        {
            var userRole = new UserRole
            {
                FkUserId = user.Id,
                FkRoleId = role.Id,
                CreatedBy = createdBy,
                DateCreated = DateTime.UtcNow,
                Active = true,
                Deleted = false
            };
            await context.UserRoles.AddAsync(userRole);
            await context.SaveChangesAsync();
            logger.LogInformation($"Assigned role {roleName} to user {user.Id}");
        }

        // Step 3: Get permission names for this role from Consts.RolePermissions
        var permissionNames = GetPermissionNamesByRole(roleName);
        if (permissionNames == null || !permissionNames.Any())
        {
            logger.LogWarning($"No permissions found for role {roleName}");
            return;
        }

        logger.LogInformation($"Found {permissionNames.Count} permissions for role {roleName}");

        // Step 4: Get all these permissions from the database
        var permissions = await context.Permissions
            .Where(p => permissionNames.Contains(p.Name) && !p.Deleted && p.Active)
            .ToListAsync();

        if (!permissions.Any())
        {
            logger.LogWarning($"No matching permissions found in database for role {roleName}");
            return;
        }

        logger.LogInformation($"Found {permissions.Count} permissions in database");

        // Step 5: Assign each permission to the user via UserPermission
        foreach (var permission in permissions)
        {
            // Check if user already has this permission
            var existingUserPermission = await context.UserPermissions
                .FirstOrDefaultAsync(up => up.FkUserId == user.Id && 
                                           up.FkPermissionId == permission.Id && 
                                           !up.Deleted);

            if (existingUserPermission == null)
            {
                var userPermission = new UserPermission
                {
                    FkUserId = user.Id,
                    FkPermissionId = permission.Id,
                    CreatedBy = createdBy,
                    DateCreated = DateTime.UtcNow,
                    Active = true,
                    Deleted = false
                };

                await context.UserPermissions.AddAsync(userPermission);
                logger.LogDebug($"Assigned permission {permission.Name} to user {user.Id}");
            }
        }

        await context.SaveChangesAsync();
        logger.LogInformation($"Successfully assigned {permissions.Count} permissions to user {user.Id} for role {roleName}");
    }

    /// <summary>
    /// Assigns multiple roles to a user and all their permissions
    /// </summary>
    public async Task AssignMultipleRolesWithPermissionsToUserAsync(Users user, List<string> roleNames, string createdBy)
    {
        logger.LogInformation($"Assigning {roleNames.Count} roles with permissions to user {user.Id}");

        foreach (var roleName in roleNames)
        {
            await AssignRoleWithPermissionsToUserAsync(user, roleName, createdBy);
        }

        logger.LogInformation($"Successfully assigned all roles and permissions to user {user.Id}");
    }

    /// <summary>
    /// Syncs user permissions based on their current roles
    /// </summary>
    public async Task SyncUserPermissionsFromRolesAsync(Users user, string updatedBy)
    {
        logger.LogInformation($"Syncing permissions for user {user.Id} based on their roles");

        // Get all user's current roles
        var userRoles = await context.UserRoles
            .Include(ur => ur.FkRole)
            .Where(ur => ur.FkUserId == user.Id && !ur.Deleted && ur.Active)
            .ToListAsync();

        if (!userRoles.Any())
        {
            logger.LogWarning($"User {user.Id} has no roles assigned");
            return;
        }

        // Remove all current permissions
        var currentPermissions = await context.UserPermissions
            .Where(up => up.FkUserId == user.Id)
            .ToListAsync();

        context.UserPermissions.RemoveRange(currentPermissions);
        await context.SaveChangesAsync();

        // Re-assign permissions based on current roles
        foreach (var userRole in userRoles)
        {
            var roleName = userRole.FkRole?.Name;
            if (string.IsNullOrEmpty(roleName)) continue;

            var permissionNames = GetPermissionNamesByRole(roleName);
            if (permissionNames == null || !permissionNames.Any()) continue;

            var permissions = await context.Permissions
                .Where(p => permissionNames.Contains(p.Name) && !p.Deleted && p.Active)
                .ToListAsync();

            foreach (var permission in permissions)
            {
                var userPermission = new UserPermission
                {
                    FkUserId = user.Id,
                    FkPermissionId = permission.Id,
                    CreatedBy = updatedBy,
                    DateCreated = DateTime.UtcNow,
                    Active = true,
                    Deleted = false
                };

                await context.UserPermissions.AddAsync(userPermission);
            }
        }

        await context.SaveChangesAsync();
        logger.LogInformation($"Successfully synced permissions for user {user.Id}");
    }

    /// <summary>
    /// Removes all permissions from a user
    /// </summary>
    public async Task RemoveAllUserPermissionsAsync(Users user)
    {
        logger.LogInformation($"Removing all permissions from user {user.Id}");

        var userPermissions = await context.UserPermissions
            .Where(up => up.FkUserId == user.Id)
            .ToListAsync();

        context.UserPermissions.RemoveRange(userPermissions);
        await context.SaveChangesAsync();

        logger.LogInformation($"Removed {userPermissions.Count} permissions from user {user.Id}");
    }

    /// <summary>
    /// Gets all effective permissions for a user (from both roles and direct assignments)
    /// </summary>
    public async Task<List<Permission>> GetUserEffectivePermissionsAsync(Guid userId)
    {
        var permissions = await context.UserPermissions
            .Include(up => up.FkPermission)
            .Where(up => up.FkUserId == userId && !up.Deleted && up.Active)
            .Select(up => up.FkPermission)
            .Where(p => p != null && !p.Deleted && p.Active)
            .Distinct()
            .ToListAsync();

        return permissions!;
    }

    /// <summary>
    /// Gets permission names for a user
    /// </summary>
    public async Task<List<string>> GetUserPermissionNamesAsync(Guid userId)
    {
        var permissions = await GetUserEffectivePermissionsAsync(userId);
        return permissions.Select(p => p.Name).ToList();
    }

    #endregion

    #region Helper/Convenience Methods (Backward Compatibility)

    /// <summary>
    /// Sets up a user as Tenant Admin with all tenant admin permissions
    /// </summary>
    public async Task SetupTenantAdminAsync(Users user, Tenants tenant, string createdBy)
    {
        logger.LogInformation($"Setting up user {user.Id} as Tenant Admin for tenant {tenant.Id}");
        await AssignRoleWithPermissionsToUserAsync(user, Consts.Roles.TenantAdmin, createdBy);
    }

    /// <summary>
    /// Sets up a user with default user permissions
    /// </summary>
    public async Task SetupDefaultUserAsync(Users user, Tenants? tenant, string createdBy)
    {
        logger.LogInformation($"Setting up user {user.Id} as Default User");
        await AssignRoleWithPermissionsToUserAsync(user, Consts.Roles.DefaultUser, createdBy);
    }

    /// <summary>
    /// Sets up a user with multiple roles
    /// </summary>
    public async Task SetupUserWithRolesAsync(Users user, List<string> roleNames, Tenants? tenant, string createdBy)
    {
        logger.LogInformation($"Setting up user {user.Id} with roles: {string.Join(", ", roleNames)}");
        await AssignMultipleRolesWithPermissionsToUserAsync(user, roleNames, createdBy);
    }

    /// <summary>
    /// Initializes default permissions for a new tenant (currently a placeholder)
    /// Can be extended to create tenant-specific configurations
    /// </summary>
    public async Task InitializeDefaultTenantPermissionsAsync(Tenants tenant, string createdBy)
    {
        logger.LogInformation($"Initializing default permissions for tenant {tenant.Id}");

        await Task.CompletedTask;
        logger.LogInformation($"Default permissions initialized for tenant {tenant.Id}");
    }

    public async Task SetupCustomerAsync(Users user, string createdBy)
    {
        logger.LogInformation($"Setting up user {user.Id} as CUSTOMER");
        await AssignRoleWithPermissionsToUserAsync(user, Consts.Roles.Customer, createdBy);
    }

    
    
    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Gets permission names from Consts.RolePermissions based on role name
    /// </summary>
    private static readonly IReadOnlyDictionary<string, List<string>> RolePermissionMap =
        new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            // PLATFORM
            [Consts.Roles.PlatformSuperAdmin] = Consts.RolePermissions.PlatformSuperAdmin,
            [Consts.Roles.PlatformSupport]    = Consts.RolePermissions.PlatformSupport,
            [Consts.Roles.PlatformAudit]      = Consts.RolePermissions.PlatformAudit,

            // TENANT
            [Consts.Roles.TenantOwner]   = Consts.RolePermissions.TenantOwner,
            [Consts.Roles.TenantAdmin]   = Consts.RolePermissions.TenantAdmin,
            [Consts.Roles.TenantManager] = Consts.RolePermissions.TenantManager,
            [Consts.Roles.TenantStaff]   = Consts.RolePermissions.TenantStaff,
            [Consts.Roles.TenantFinance] = Consts.RolePermissions.TenantFinance,

            // CUSTOMER
            [Consts.Roles.Customer] = Consts.RolePermissions.Customer
        };

    private List<string>? GetPermissionNamesByRole(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName)) return null;

        return RolePermissionMap.TryGetValue(roleName, out var permissions)
            ? permissions
            : null;
    }


    #endregion
}