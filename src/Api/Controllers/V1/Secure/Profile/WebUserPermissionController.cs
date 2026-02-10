using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Repository.Permission;
using Bookazone.Application.Interfaces.Repository.Profile;
using Bookazone.Domain.Entities.Permissions;
using Bookazone.Infrastructure.Authorization;
using Functions = Bookazone.Application.Common.Shared.Utils.Functions;

namespace Bookazone.Api.Controllers.V1.Secure.Profile;

[Authorize]
[ApiController]
[Route(RouteWebVersion1.Secure.UserPermission.Base)]
public class WebUserPermissionController(
    IUserPermissionRepository userPermissionRepo,
    IPermissionRepository permissionRepository,
    Functions functions,
    IUserRepository userRepository,
    ILogger<WebUserPermissionController> logger
) : ControllerBase
{
    [HttpGet(RouteWebVersion1.Secure.UserPermission.GetByUser)]
    [PermissionAuthorize(Consts.Permissions.TenantPermissionsManage)] // new Bookazone permission
    public async Task<Task<ApiResult>> GetUserPermissions([FromQuery] Guid userId)
    {
        var permissions = userPermissionRepo.FindByUserId(userId);
        return Task.FromResult(ApiResponse.Success(permissions));
    }

    [HttpPost(RouteWebVersion1.Secure.UserPermission.Assign)]
// [PermissionAuthorize(Consts.Permissions.USER_MANAGE_PERMISSIONS)]
    public async Task<ApiResult> AssignPermissions([FromBody] AssignPermissionsRequest request)
    {
        if (request.UserId == Guid.Empty || request.FkPermission == null || request.FkPermission.Count == 0) 
            return ApiResponse.Error(ErrorHttp.BadRequest);
    
        var checkUser = userRepository.Find(request.UserId);
        if (checkUser == null) 
            return ApiResponse.Error(ErrorHttp.NotFound);
        var existingPermissions = userPermissionRepo.FindByUserId(request.UserId);
        var existingPermissionIds = existingPermissions.Select(up => up.FkPermission.Id).ToHashSet();
    
        var userPermissions = new List<UserPermission>();
    
        foreach (var permissionId in request.FkPermission)
        {
            if (existingPermissionIds.Contains(permissionId))
            {
                continue;
            }
        
            var permission = await permissionRepository.GetByIdAsync(permissionId);
            if (permission == null)
            {
                return ApiResponse.Error(ErrorHttp.NotFound);
            }
        
            var createUserPermission = new UserPermission
            {
                FkPermissionId = permissionId,
                FkUserId = checkUser.Id, 
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                Active = true,
                Deleted = false,
                CreatedBy = User.Identity?.Name ?? "system"
            };
            userPermissionRepo.Create(createUserPermission);
            userPermissions.Add(createUserPermission);
        }
        return ApiResponse.Success(userPermissions);
    }
    
    
    [HttpDelete(RouteWebVersion1.Secure.UserPermission.Remove)]
    [PermissionAuthorize(Consts.Permissions.TenantPermissionsManage)] // new Bookazone permission
    public async Task<ApiResult> SyncUserPermissions([FromBody] SyncPermissionsRequest request)
    {
        var result = await userPermissionRepo.SyncUserPermissionsAsync(
            request.UserId, 
            request.PermissionIds, 
            User.Identity?.Name ?? "system"
        );
        
        return ApiResponse.Success(result) ;
    }

}

public class AssignPermissionsRequest
{
    public Guid UserId { get; set; }
    public List<Guid>? FkPermission { get; set; }
}



public record SyncPermissionsRequest(Guid UserId, List<Guid> PermissionIds);
