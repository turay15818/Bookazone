using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Repository.Permission;
using Functions = Bookazone.Application.Common.Shared.Utils.Functions;

namespace Bookazone.Api.Controllers.V1.Secure.Profile;

[Authorize]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.WebVersion1.Slug)]
[Route(RouteWebVersion1.Secure.UserRole.Base)]
public class WebUserRoleController(
    Functions functions,
    ILogger<WebUserRoleController> logger,
    IUserRoleRepository userRoleRepository,
    IRoleRepository roleRepository
) : ControllerBase
{
    [HttpGet]
    [Route(RouteWebVersion1.Secure.UserRole.All)]
    public async Task<ApiResult> All()
    {
        try
        {
            var user = functions.GetUser(User.Identity?.Name);
            if (user == null)
                return ApiResponse.Error(ErrorHttp.TokenExpired);

            var roles = await userRoleRepository.AllAsync();
            return ApiResponse.Success(roles);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting all user roles");
            return ApiResponse.Error(ErrorHttp.Error, e);
        }
    }


    [HttpGet]
    [Route(RouteWebVersion1.Secure.UserRole.GetByUser)]
    public async Task<ApiResult> GetByUser(Guid userId)
    {
        try
        {
            var user = functions.GetUser(User.Identity?.Name);
            if (user == null)
                return ApiResponse.Error(ErrorHttp.TokenExpired);

            var userRoles = await userRoleRepository.GetRolesByUserAsync(userId);
            return ApiResponse.Success(userRoles);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting roles for user {UserId}", userId);
            return ApiResponse.Error(ErrorHttp.Error, e);
        }
    }
    
    [HttpPost]
    [Route(RouteWebVersion1.Secure.UserRole.Assign)]
    public async Task<ApiResult> AssignRoles(Guid userId, [FromBody] List<Guid> roleIds)
    {
        try
        {
            var user = functions.GetUser(User.Identity?.Name);
            if (user == null)
                return ApiResponse.Error(ErrorHttp.TokenExpired);
            var result = await userRoleRepository.AssignRolesToUserAsync(
                userId,
                roleIds,
                user?.Username
            );
            return ApiResponse.Success(result);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error assigning roles to user {UserId}", userId);
            return ApiResponse.Error(ErrorHttp.Error, e);
        }
    }

    // ✅ Remove specific or all roles from a user
    [HttpPost]
    [Route(RouteWebVersion1.Secure.UserRole.Remove)]
    public async Task<ApiResult> RemoveRoles(Guid userId, [FromBody] List<Guid>? roleIds = null)
    {
        try
        {
            var user = functions.GetUser(User.Identity?.Name);
            if (user == null)
                return ApiResponse.Error(ErrorHttp.TokenExpired);

            var result = await userRoleRepository.RemoveRolesFromUserAsync(userId, roleIds);
            return ApiResponse.Success(result);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error removing roles from user {UserId}", userId);
            return ApiResponse.Error(ErrorHttp.Error, e);
        }
    }

    // ✅ Check if user has a specific role
    [HttpGet]
    [Route(RouteWebVersion1.Secure.UserRole.HasRole)]
    public async Task<ApiResult> HasRole(Guid userId, string roleName)
    {
        try
        {
            var user = functions.GetUser(User.Identity?.Name);
            if (user == null)
                return ApiResponse.Error(ErrorHttp.TokenExpired);

            var hasRole = await userRoleRepository.UserHasRoleAsync(userId, roleName);
            return ApiResponse.Success(hasRole);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error checking if user {UserId} has role {RoleName}",
                userId,
                roleName
            );
            return ApiResponse.Error(ErrorHttp.Error, e);
        }
    }

    // ✅ Toggle user-role status (activate/deactivate)
    [HttpPost]
    [Route(RouteWebVersion1.Secure.UserRole.Status)]
    public async Task<ApiResult> Status(Guid id, [FromBody] bool active)
    {
        try
        {
            var user = functions.GetUser(User.Identity?.Name);
            if (user == null)
                return ApiResponse.Error(ErrorHttp.TokenExpired);

            var result = await userRoleRepository.StatusAsync(id, active);
            return ApiResponse.Success(result);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating user role status {Id}", id);
            return ApiResponse.Error(ErrorHttp.Error, e);
        }
    }
}
