using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Api.Controllers.V1.Config.Request;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Services;
using Bookazone.Infrastructure.Authorization;

namespace Bookazone.Api.Controllers.V1.Secure.Tenant;

[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.WebVersion1.Slug)]
[Route(RouteWebVersion1.Secure.Tenant.Base)]
public class TenantController(ITenantService tenantService, ILogger<TenantController> logger)
    : ControllerBase
{
    [Authorize]
    [HttpGet]
    [PermissionAuthorize(Consts.Permissions.TenantView)]
    [Route(RouteWebVersion1.Secure.Tenant.Profile)]
    public async Task<ApiResult> Profile()
    {
        try
        {
            return await tenantService.GetProfileAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant profile");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [Authorize]
    [HttpGet]
    [PermissionAuthorize(Consts.Permissions.TenantView)]
    [Route(RouteWebVersion1.Secure.Tenant.All)]
    public async Task<ApiResult> All(string? q, int pageSize = 50, int pageIndex = 1)
    {
        try
        {
            return await tenantService.GetAllAsync(q, pageSize, pageIndex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all tenants");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [Authorize]
    [HttpGet]
    [PermissionAuthorize(Consts.Permissions.TenantView)]
    [Route(RouteWebVersion1.Secure.Tenant.Find)]
    public async Task<ApiResult> Find(Guid id)
    {
        try
        {
            return await tenantService.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error finding tenant {Id}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    // [PermissionAuthorize(Consts.Permissions.TENANT_CREATE)]
    [Route(RouteWebVersion1.Secure.Tenant.Create)]
    public async Task<ApiResult> Create([FromBody] TenantRequest request)
    {
        try
        {
            return await tenantService.CreateAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating tenant");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [AllowAnonymous]
    [Route(RouteWebVersion1.Secure.Tenant.Verify)]
    public async Task<ApiResult> Verify([FromBody] VerifyAccountRequest request)
    {
        try
        {
            return await tenantService.VerifyAccountAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verifying tenant account");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPut]
    [PermissionAuthorize(Consts.Permissions.TenantUpdate)]
    [Route(RouteWebVersion1.Secure.Tenant.Update)]
    public async Task<ApiResult> Update([FromForm] TenantUpdateRequest request)
    {
        try
        {
            return await tenantService.UpdateAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating tenant");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpDelete]
    [PermissionAuthorize(Consts.Permissions.TenantDelete)]
    [Route(RouteWebVersion1.Secure.Tenant.Delete)]
    public async Task<ApiResult> Delete(Guid id)
    {
        try
        {
            return await tenantService.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting tenant {Id}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
