using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Api.Controllers.V1.Config.Request;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Converter;
using Bookazone.Application.Interfaces.Repository.Permission;
using Bookazone.Domain.Entities.Permissions;

namespace Bookazone.Api.Controllers.V1.Secure.Profile;

[Authorize]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.WebVersion1.Slug)]
[Route(RouteWebVersion1.Secure.Role.Base)]
public class WebRoleController(
    ILogger<WebRoleController> logger,
    Application.Common.Shared.Utils.Functions functions,
    IRoleRepository roleRepository
) : ControllerBase
{
    [HttpGet]
    [Route(RouteWebVersion1.Secure.Role.All)]
    public async Task<ApiResult> All()
    {
        try
        {
            var roles = await roleRepository.AllAsync();
            return ApiResponse.Success(roles.Select(RoleConverter.ToDto));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting all roles");
            return ApiResponse.Error(ErrorHttp.Error, e);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Secure.Role.Find)]
    public async Task<ApiResult> Find(Guid id)
    {
        try
        {
            var role = await roleRepository.FindAsync(id);
            if (role == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            return ApiResponse.Success(role);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error finding role {Id}", id);
            return ApiResponse.Error(ErrorHttp.Error, e);
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.Role.Create)]
    public async Task<ApiResult> Create([FromBody] RoleRequest role)
    {
        try
        {
            var user = functions.GetUser(User.Identity?.Name);
            if (user == null)
                return ApiResponse.Error(ErrorHttp.TokenExpired);
            var checkRole = await roleRepository.FindByNameAsync(role.Name);
            if (checkRole != null)
                return ApiResponse.Error(
                    ErrorHttp.AlreadyExists,
                    new Exception("The role with the same name already exists")
                );

            var createRole = new Role
            {
                Name = role.Name,
                Description = role.Description,
                Slug = role.Name,
                IsSystem = role.IsSystem,
                CreatedBy = user.Username,
                DateCreated = DateTime.UtcNow,
                Active = true,
                Deleted = false,
            };
            var createdRole = await roleRepository.CreateAsync(createRole);
            return createdRole == null
                ? ApiResponse.Error(ErrorHttp.DbCreateError)
                : ApiResponse.Success(createdRole);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating role");
            return ApiResponse.Error(ErrorHttp.Error, e);
        }
    }

    [HttpPatch]
    [Route(RouteWebVersion1.Secure.Role.Update)]
    public async Task<ApiResult> Update([FromBody] Role role)
    {
        try
        {
            var user = functions.GetUser(User.Identity?.Name);
            if (user == null)
                return ApiResponse.Error(ErrorHttp.TokenExpired);

            role.UpdatedBy = user.Username;
            var updatedRole = await roleRepository.UpdateAsync(role);

            return updatedRole == null
                ? ApiResponse.Error(ErrorHttp.DbCreateError)
                : ApiResponse.Success(updatedRole);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating role");
            return ApiResponse.Error(ErrorHttp.Error, e);
        }
    }

    [HttpDelete]
    [Route(RouteWebVersion1.Secure.Role.Delete)]
    public async Task<ApiResult> Delete(Guid id)
    {
        try
        {
            var deleted = await roleRepository.DeleteAsync(id);
            return deleted ? ApiResponse.Success(true) : ApiResponse.Error(ErrorHttp.DbDeleteError);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting role {Id}", id);
            return ApiResponse.Error(ErrorHttp.Error, e);
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.Role.Status)]
    public async Task<ApiResult> Status(Guid id, [FromBody] bool active)
    {
        try
        {
            var updated = await roleRepository.StatusAsync(id, active);
            return updated ? ApiResponse.Success(true) : ApiResponse.Error(ErrorHttp.DbCreateError);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating role status {Id}", id);
            return ApiResponse.Error(ErrorHttp.Error, e);
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.Role.AssignPermissions)]
    public async Task<ApiResult> AssignPermissions(Guid roleId, [FromBody] List<Guid> permissionIds)
    {
        try
        {
            var user = functions.GetUser(User.Identity?.Name);
            if (user == null)
                return ApiResponse.Error(ErrorHttp.TokenExpired);

            var result = await roleRepository.AssignPermissionsAsync(
                roleId,
                permissionIds,
                user.Username
            );
            return result
                ? ApiResponse.Success("Permissions assigned successfully")
                : ApiResponse.Error(ErrorHttp.DbCreateError);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error assigning permissions to role {Id}", roleId);
            return ApiResponse.Error(ErrorHttp.Error, e);
        }
    }
    
    [HttpGet]
    [Route(RouteWebVersion1.Secure.Role.RolesWithPermissions)]
    public async Task<ApiResult> GetRolesWithPermissions()
    {
        try
        {
            var roles = await roleRepository.GetAllRolesWithPermissionsAsync();

            if (roles == null || roles.Count == 0)
                return ApiResponse.Success(new List<object>());

            return ApiResponse.Success(roles);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching roles with permissions");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    
    
    [HttpPost]
    [Route(RouteWebVersion1.Secure.Role.PermissionsUpdate)]
    public async Task<ApiResult> UpdateRolePermissions([FromBody] RolePermissionUpdateRequest request)
    {
        try
        {
            var currentUser = functions.GetUser(User.Identity?.Name);
            if (currentUser == null)
                return ApiResponse.Error(ErrorHttp.TokenExpired);

            var success = await roleRepository.UpdateRolePermissionsAsync(
                request.RoleId,
                request.PermissionIds,
                currentUser.Username
            );

            var message = success
                ? "Role permissions updated successfully."
                : "No permission changes detected.";

            return ApiResponse.Success(new { message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating role permissions for {RoleId}", request.RoleId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    
[HttpGet]
[Route(RouteWebVersion1.Secure.Role.AllPermissions)]
public async Task<ApiResult> GetAllRolePermissions()
{
    try
    {
        var roles = await roleRepository.AllAsync();
        var result = roles.Select(role => new
        {
            RoleId = role.Id,
            RoleName = role.Name,
            Permissions = role.RolePermissions
                .Where(rp => rp.FkPermission != null && rp.FkPermission.Deleted != true)
                .Select(rp => new
                {
                    rp.FkPermission!.Id,
                    rp.FkPermission.Name,
                    rp.FkPermission.Description
                })
                .ToList()
        });

        return ApiResponse.Success(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error retrieving all role permissions");
        return ApiResponse.Error(ErrorHttp.Error, ex);
    }
}


// ✅ 2️⃣ Get permissions for a specific role (by ID)
[HttpGet]
[Route(RouteWebVersion1.Secure.Role.GetPermissions)]
public async Task<ApiResult> GetPermissionsByRole(Guid roleId)
{
    try
    {
        var permissions = await roleRepository.GetPermissionsByRoleAsync(roleId);
        if (permissions == null || permissions.Count == 0)
            return ApiResponse.Success(new List<object>()); // empty list if none

        var dto = permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description
        }).ToList();

        return ApiResponse.Success(dto);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error fetching permissions for role {RoleId}", roleId);
        return ApiResponse.Error(ErrorHttp.Error, ex);
    }
}


    
    
    
    
    
}
