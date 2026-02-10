using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Api.Controllers.V1.Config.Request;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Services.Booking;
using Bookazone.Infrastructure.Authorization;

namespace Bookazone.Api.Controllers.V1.Secure.BookingConfig;

[Authorize]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.WebVersion1.Slug)]
[Route(RouteWebVersion1.Secure.BookingConfig.SportResource.Base)]
public class SportResourceController(
    IBookingConfigurationService bookingConfigurationService,
    ILogger<SportResourceController> logger)
    : ControllerBase
{
    [HttpGet]
    [PermissionAuthorize(Consts.Permissions.ResourceView)]
    [Route(RouteWebVersion1.Secure.BookingConfig.SportResource.ByTenant)]
    public async Task<ApiResult> ByTenant(Guid tenantId)
    {
        try
        {
            return await bookingConfigurationService.GetSportResourcesByTenantAsync(tenantId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sport resources for tenant {TenantId}", tenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [PermissionAuthorize(Consts.Permissions.ResourceView)]
    [Route(RouteWebVersion1.Secure.BookingConfig.SportResource.ByType)]
    public async Task<ApiResult> ByType(Guid tenantSportTypeId)
    {
        try
        {
            return await bookingConfigurationService.GetSportResourcesByTypeAsync(tenantSportTypeId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sport resources for sport type {SportTypeId}", tenantSportTypeId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceCreate)]
    [Route(RouteWebVersion1.Secure.BookingConfig.SportResource.Create)]
    public async Task<ApiResult> Create([FromBody] SportResourceCreateRequest request)
    {
        try
        {
            return await bookingConfigurationService.CreateSportResourceAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating sport resource for tenant {TenantId}", request.TenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPut]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.BookingConfig.SportResource.Update)]
    public async Task<ApiResult> Update([FromBody] SportResourceUpdateRequest request)
    {
        try
        {
            return await bookingConfigurationService.UpdateSportResourceAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating sport resource {ResourceId}", request.Id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpDelete]
    [PermissionAuthorize(Consts.Permissions.ResourceDelete)]
    [Route(RouteWebVersion1.Secure.BookingConfig.SportResource.Delete)]
    public async Task<ApiResult> Delete(Guid id)
    {
        try
        {
            return await bookingConfigurationService.DeleteSportResourceAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting sport resource {ResourceId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
