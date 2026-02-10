using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Api.Controllers.V1.Config.Request;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Repository.Permission;
using Bookazone.Domain.Entities.Permissions;
using Functions = Bookazone.Application.Common.Shared.Utils.Functions;

namespace Bookazone.Api.Controllers.V1.Secure.Profile;


[Authorize]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.WebVersion1.Slug)]
[Route(RouteWebVersion1.Secure.Permission.Base)]
public class WebPermissionController(
    Functions functions,
    IRoleRepository roleRepository,
    ILogger<WebPermissionController> logger,
    IPermissionRepository permissionRepository
) : ControllerBase
{
    [HttpGet]
    [Route(RouteWebVersion1.Secure.Permission.All)]
    public async Task<ApiResult> All()
    {
        try
        {
            var user = functions.GetUser(User.Identity?.Name);
            logger.LogError("User Information: {User}", user);
            if (user == null) return await Task.FromResult(ApiResponse.Error(ErrorHttp.TokenExpired));
            var permissions = permissionRepository.GetAllAsync().Result;
            return ApiResponse.Success(permissions);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting all permissions");
            return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error, e));
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Secure.Permission.Find)]
    public async Task<ApiResult> Find(Guid id)
    {
        try
        {
            var user = functions.GetUser(User.Identity?.Name);
            logger.LogError("User Information: {User}", user);
            if (user == null) return await Task.FromResult(ApiResponse.Error(ErrorHttp.TokenExpired));
            var permission = permissionRepository.GetByIdAsync(id).Result;
            if (permission == null)
                return await Task.FromResult(ApiResponse.Error(ErrorHttp.NotFound));

            return await Task.FromResult(ApiResponse.Success(permission));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error finding permission with id {Id}", id);
            return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error, e));
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Secure.Permission.FindByName)]
    public async Task<ApiResult> FindByName(string name)
    {
        try
        {
            var user = functions.GetUser(User.Identity?.Name);
            logger.LogError("User Information: {User}", user);
            if (user == null) return await Task.FromResult(ApiResponse.Error(ErrorHttp.TokenExpired));
            var permission = permissionRepository.GetByNameAsync(name).Result;
            return await Task.FromResult(ApiResponse.Success(permission));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error finding permission by name {Name}", name);
            return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error, e));
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.Permission.Create)]
public async Task<ApiResult> Create([FromBody] PermissionRequest permissionRequest)
{
    try
    {
        var checkRole = roleRepository.FindAsync(permissionRequest.FkRole).Result;
        if (checkRole == null) return ApiResponse.Error(ErrorHttp.NotFound, new Exception("Role not found"));
        var createdPermissions = new List<Permission>();
        foreach (var permission in permissionRequest.Permissions)
        {
            try
            {
                var existingPermission = permissionRepository.GetByNameAsync(permission.Name).Result;
                if (existingPermission != null)
                {
                    createdPermissions.Add(existingPermission);
                    continue;
                }
                var newPermission = new Permission
                {
                    Name = permission.Name ?? "",
                    Slug = permission.Name?.ToLower().Replace(" ", "_") ?? "",
                    Description = permission.Description ?? $"Used to {permission.Name?.Replace("_", " ")}",
                    DateCreated = DateTime.UtcNow,
                    DateUpdated = DateTime.UtcNow,
                    IsSystem = permission.IsSystem,
                    Active = true,
                    Deleted = false,
                    CreatedBy = permission.IsSystem ? "System" : "User"
                };
                var createdPermission = permissionRepository.CreateAsync(newPermission).Result;
                createdPermissions.Add(createdPermission);
            }
            catch (Exception innerEx)
            {
                logger.LogError(innerEx, "Error creating permission for role {RoleId} and permission {PermissionName}", permissionRequest.FkRole, permission.Name);
            }
        }
        return ApiResponse.Success(createdPermissions);
    }
    catch (Exception e)
    {
        return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error, e));
    }
}

    
    
    
    
    
    [HttpPatch]
    [Route(RouteWebVersion1.Secure.Permission.Update)]
    public async Task<ApiResult> Update([FromBody] Permission permission)
    {
        try
        {
            var user = functions.GetUser(User.Identity?.Name);
            logger.LogError("User Information: {User}", user);
            if (user == null) return await Task.FromResult(ApiResponse.Error(ErrorHttp.TokenExpired));
            if (permission == null) return await Task.FromResult(ApiResponse.Error(ErrorHttp.BadRequest));
            var updatedPermission = permissionRepository.UpdateAsync(permission).Result;
            if (updatedPermission == null) return await Task.FromResult(ApiResponse.Error(ErrorHttp.DbCreateError));
            return await Task.FromResult(ApiResponse.Success(updatedPermission));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating permission");
            return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error, e));
        }
    }

    [HttpDelete]
    [Route(RouteWebVersion1.Secure.Permission.Delete)]
    public async Task<ApiResult> Delete(Guid id)
    {
        try
        {
            var user = functions.GetUser(User.Identity?.Name);
            logger.LogError("User Information: {User}", user);
            if (user == null) return await Task.FromResult(ApiResponse.Error(ErrorHttp.TokenExpired));
            var result = permissionRepository.DeleteAsync(id);
            return await Task.FromResult(ApiResponse.Success(result));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting permission with id {Id}", id);
            return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error, e));
        }
    }
    
}