using Microsoft.EntityFrameworkCore;
using Bookazone.Application.DTOs;
using Bookazone.Application.Interfaces.Repository.Permission;
using Bookazone.Application.Interfaces.Repository.Security;
using Bookazone.Domain.Entities.Permissions;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Infrastructure.Persistence;
using Bookazone.Infrastructure.Persistence.DbContext;

namespace Bookazone.Infrastructure.Services.Security;

public class UserAccessService(
    IUserRoleRepository userRoleRepository,
    IUserPermissionRepository userPermissionRepository,
    IRoleRepository roleRepository,
    BookazoneDbContext  BookazoneDbContext,
    IPermissionRepository permissionRepository,
    ILogger<UserAccessService> logger
) : IUserAccessService
{
    
    
    public async Task<UserAccessDto?> GetUserAccessAsync(Guid userId)
    {
        var user = await BookazoneDbContext.Users
            .Include(u => u.UserPermissions)
            .ThenInclude(up => up.FkPermission)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.FkRole)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.FkPermission)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.Deleted);
        if (user == null)
            return null;
        var dto = new UserAccessDto
        {
            UserId = user.Id,
            Username = user.Username ?? "",
            DirectPermissions = user.UserPermissions
                .Where(up => up.Active && !up.Deleted)
                .Select(up => up.FkPermission.Name)
                .Distinct()
                .ToList(),

            Roles = user.UserRoles
                .Where(ur => ur.Active && !ur.Deleted)
                .Select(ur => ur.FkRole.Name)
                .Distinct()
                .ToList(),

            RolePermissions = user.UserRoles
                .Where(ur => ur.Active && !ur.Deleted)
                .SelectMany(ur => ur.FkRole.RolePermissions
                    .Where(rp => rp.Active && !rp.Deleted)
                    .Select(rp => rp.FkPermission.Name))
                .Distinct()
                .ToList()
        };

        return dto;
    }
    public async Task<(bool rolesChanged, bool permissionsChanged)> UpdateUserAccessAsync(
        Users userToUpdate,
        List<string>? newRoles,
        List<string>? newPermissions,
        string? updatedBy
    )
    {
        bool rolesChanged = false;
        bool permissionsChanged = false;

        var existingRoles = userRoleRepository.FindByUserId(userToUpdate.Id);
        var existingPerms = userPermissionRepository.FindByUserId(userToUpdate.Id);

        // 🧩 ROLES SECTION
        if (newRoles != null)
        {
            var providedRoles = newRoles.Distinct().ToList();
            var currentRoles = existingRoles
                .Where(r => r.FkRole != null && r.Active)
                .Select(r => r.FkRole!.Name)
                .ToList();

            // Remove all if none provided
            if (!providedRoles.Any())
            {
                foreach (var userRole in existingRoles)
                {
                    userRole.Active = false;
                    userRole.Deleted = true;
                    userRole.DeletedBy = updatedBy;
                    userRole.DateDeleted = DateTime.UtcNow;
                    userRoleRepository.Update(userRole);
                    rolesChanged = true;
                }
            }
            else
            {
                // Add / Reactivate
                var toAddRoles = providedRoles.Except(currentRoles).ToList();
                foreach (var roleName in toAddRoles)
                {
                    var roleEntity = await roleRepository.FindByNameAsync(roleName);
                    if (roleEntity != null)
                    {
                        var existingUserRole = existingRoles.FirstOrDefault(r =>
                            r.FkRole?.Id == roleEntity.Id
                        );
                        if (existingUserRole != null)
                        {
                            existingUserRole.Active = true;
                            existingUserRole.Deleted = false;
                            existingUserRole.UpdatedBy = updatedBy;
                            existingUserRole.DateUpdated = DateTime.UtcNow;
                            userRoleRepository.Update(existingUserRole);
                        }
                        else
                        {
                            var newUserRole = new UserRole
                            {
                                FkRole = roleEntity,
                                FkUser = userToUpdate,
                                Active = true,
                                CreatedBy = updatedBy,
                                DateCreated = DateTime.UtcNow,
                            };
                            await userRoleRepository.CreateAsync(newUserRole);
                        }
                        rolesChanged = true;
                    }
                }

                // Disable missing roles
                var toRemoveRoles = currentRoles.Except(providedRoles).ToList();
                foreach (var roleName in toRemoveRoles)
                {
                    var roleEntity = await roleRepository.FindByNameAsync(roleName);
                    if (roleEntity != null)
                    {
                        var userRole = existingRoles.FirstOrDefault(r =>
                            r.FkRole?.Id == roleEntity.Id
                        );
                        if (userRole != null)
                        {
                            userRole.Active = false;
                            userRole.Deleted = true;
                            userRole.DeletedBy = updatedBy;
                            userRole.DateDeleted = DateTime.UtcNow;
                            userRoleRepository.Update(userRole);
                            rolesChanged = true;
                        }
                    }
                }
            }
        }

        // 🧩 PERMISSIONS SECTION
        if (newPermissions != null)
        {
            var providedPerms = newPermissions.Distinct().ToList();
            var currentPerms = existingPerms
                .Where(p => p.FkPermission != null && p.Active)
                .Select(p => p.FkPermission!.Name)
                .ToList();

            // Add or reactivate
            var toAdd = providedPerms.Except(currentPerms).ToList();
            foreach (var perm in toAdd)
            {
                var permEntity = await permissionRepository.GetByNameAsync(perm);
                if (permEntity != null)
                {
                    var existingLink = existingPerms.FirstOrDefault(p =>
                        p.FkPermission?.Id == permEntity.Id
                    );
                    if (existingLink != null)
                    {
                        existingLink.Active = true;
                        existingLink.Deleted = false;
                        existingLink.UpdatedBy = updatedBy;
                        existingLink.DateUpdated = DateTime.UtcNow;
                        userPermissionRepository.Update(existingLink);
                    }
                    else
                    {
                        var createUserPermission = new UserPermission
                        {
                            FkPermission = permEntity,
                            FkUser = userToUpdate,
                            Active = true,
                            CreatedBy = updatedBy,
                            DateCreated = DateTime.UtcNow,
                        };
                        userPermissionRepository.Create(createUserPermission);
                    }
                    permissionsChanged = true;
                }
            }

            // Disable or remove missing
            var toRemove = currentPerms.Except(providedPerms).ToList();
            foreach (var perm in toRemove)
            {
                var permEntity = await permissionRepository.GetByNameAsync(perm);
                if (permEntity != null)
                {
                    var userPerm = existingPerms.FirstOrDefault(p =>
                        p.FkPermission?.Id == permEntity.Id
                    );
                    if (userPerm != null)
                    {
                        userPerm.Active = false;
                        userPerm.Deleted = true;
                        userPerm.DeletedBy = updatedBy;
                        userPerm.DateDeleted = DateTime.UtcNow;
                        userPermissionRepository.Update(userPerm);
                        permissionsChanged = true;
                    }
                }
            }
        }

        logger.LogInformation(
            "UserAccessService updated roles={RolesChanged}, permissions={PermissionsChanged} for user {User}",
            rolesChanged,
            permissionsChanged,
            userToUpdate.Id
        );

        return (rolesChanged, permissionsChanged);
    }
    
    
    
    
    
public async Task<(Users user, bool rolesChanged, bool permissionsChanged)> CreateOrUpdateUserWithAccessAsync(
    Users userEntity,
    List<string>? roleNames,
    List<string>? permissionNames,
    string? createdBy
)
{
    bool rolesChanged = false;
    bool permissionsChanged = false;

    // --- Step 1️⃣: Handle Roles ---
    var existingRoles = userRoleRepository.FindByUserId(userEntity.Id);
    var currentRoleIds = existingRoles
        .Where(ur => ur.Active)
        .Select(ur => ur.FkRoleId)
        .ToHashSet();

    if (roleNames != null && roleNames.Any())
    {
        var selectedRoles = new List<Role>();

        foreach (var roleName in roleNames.Distinct())
        {
            var roleEntity = await roleRepository.FindByNameAsync(roleName);
            if (roleEntity == null) continue;

            selectedRoles.Add(roleEntity);

            // Check if role already exists for this user
            var existing = existingRoles.FirstOrDefault(ur => ur.FkRoleId == roleEntity.Id);
            if (existing == null)
            {
                // ✅ Create new link
                var newUserRole = new UserRole
                {
                    FkUser = userEntity,
                    FkRole = roleEntity,
                    FkRoleId = roleEntity.Id,
                    FkUserId = userEntity.Id,
                    Active = true,
                    CreatedBy = createdBy,
                    DateCreated = DateTime.UtcNow
                };
                await userRoleRepository.CreateAsync(newUserRole);
                rolesChanged = true;
            }
            else if (existing.Deleted || !existing.Active)
            {
                // ✅ Reactivate
                existing.Active = true;
                existing.Deleted = false;
                existing.UpdatedBy = createdBy;
                existing.DateUpdated = DateTime.UtcNow;
                userRoleRepository.Update(existing);
                rolesChanged = true;
            }
        }

        // ✅ Deactivate roles that are no longer selected
        var selectedRoleIds = selectedRoles.Select(r => r.Id).ToHashSet();
        foreach (var existing in existingRoles.Where(er => !selectedRoleIds.Contains(er.FkRoleId)))
        {
            existing.Active = false;
            existing.Deleted = true;
            existing.DeletedBy = createdBy;
            existing.DateDeleted = DateTime.UtcNow;
            userRoleRepository.Update(existing);
            rolesChanged = true;
        }

        // --- Step 2️⃣: Merge all permissions from active roles ---
        var allRolePerms = selectedRoles
            .SelectMany(r => r.RolePermissions)
            .Where(rp => rp.FkPermission != null && rp.Active && !rp.Deleted)
            .Select(rp => rp.FkPermission!.Name)
            .Distinct()
            .ToList();

        // Merge role permissions with direct ones
        permissionNames ??= new List<string>();
        permissionNames.AddRange(allRolePerms);
        permissionNames = permissionNames.Distinct().ToList();
    }

    // --- Step 3️⃣: Handle Direct Permissions ---
    if (permissionNames != null)
    {
        var providedPerms = permissionNames.Distinct().ToList();
        var existingPerms = userPermissionRepository.FindByUserId(userEntity.Id);

        var currentPerms = existingPerms
            .Where(p => p.FkPermission != null && p.Active)
            .Select(p => p.FkPermission!.Name)
            .ToList();

        // ✅ Add new permissions
        var toAdd = providedPerms.Except(currentPerms).ToList();
        foreach (var perm in toAdd)
        {
            var permEntity = await permissionRepository.GetByNameAsync(perm);
            if (permEntity == null) continue;

            // Prevent duplicates
            var alreadyLinked = existingPerms.Any(p => p.FkPermissionId == permEntity.Id);
            if (!alreadyLinked)
            {
                var newPerm = new UserPermission
                {
                    FkUser = userEntity,
                    FkUserId = userEntity.Id,
                    FkPermissionId = permEntity.Id,
                    Active = true,
                    CreatedBy = createdBy,
                    DateCreated = DateTime.UtcNow
                };
                userPermissionRepository.Create(newPerm);
                permissionsChanged = true;
            }
            else
            {
                var existing = existingPerms.First(p => p.FkPermissionId == permEntity.Id);
                if (!existing.Active)
                {
                    existing.Active = true;
                    existing.Deleted = false;
                    existing.UpdatedBy = createdBy;
                    existing.DateUpdated = DateTime.UtcNow;
                    userPermissionRepository.Update(existing);
                    permissionsChanged = true;
                }
            }
        }

        // ✅ Deactivate missing permissions
        var toRemove = currentPerms.Except(providedPerms).ToList();
        foreach (var perm in toRemove)
        {
            var permEntity = await permissionRepository.GetByNameAsync(perm);
            if (permEntity != null)
            {
                var existingLink = existingPerms.FirstOrDefault(p => p.FkPermissionId == permEntity.Id);
                if (existingLink != null)
                {
                    existingLink.Active = false;
                    existingLink.Deleted = true;
                    existingLink.DeletedBy = createdBy;
                    existingLink.DateDeleted = DateTime.UtcNow;
                    userPermissionRepository.Update(existingLink);
                    permissionsChanged = true;
                }
            }
        }
    }

    return (userEntity, rolesChanged, permissionsChanged);
}


    
    
    
    
}
