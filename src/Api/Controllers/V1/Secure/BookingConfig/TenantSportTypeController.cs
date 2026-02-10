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
[Route(RouteWebVersion1.Secure.BookingConfig.SportType.Base)]
public class TenantSportTypeController(
    IBookingConfigurationService bookingConfigurationService,
    ILogger<TenantSportTypeController> logger)
    : ControllerBase
{
    [HttpGet]
    [PermissionAuthorize(Consts.Permissions.ResourceView)]
    [Route(RouteWebVersion1.Secure.BookingConfig.SportType.All)]
    public async Task<ApiResult> All(Guid tenantId)
    {
        try
        {
            return await bookingConfigurationService.GetTenantSportTypesAsync(tenantId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant sport types for tenant {TenantId}", tenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceCreate)]
    [Route(RouteWebVersion1.Secure.BookingConfig.SportType.Create)]
    public async Task<ApiResult> Create([FromBody] TenantSportTypeCreateRequest request)
    {
        try
        {
            return await bookingConfigurationService.CreateTenantSportTypeAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating tenant sport type for tenant {TenantId}", request.TenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPut]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.BookingConfig.SportType.Update)]
    public async Task<ApiResult> Update([FromBody] TenantSportTypeUpdateRequest request)
    {
        try
        {
            return await bookingConfigurationService.UpdateTenantSportTypeAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating tenant sport type {SportTypeId}", request.Id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpDelete]
    [PermissionAuthorize(Consts.Permissions.ResourceDelete)]
    [Route(RouteWebVersion1.Secure.BookingConfig.SportType.Delete)]
    public async Task<ApiResult> Delete(Guid id)
    {
        try
        {
            return await bookingConfigurationService.DeleteTenantSportTypeAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting tenant sport type {SportTypeId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
