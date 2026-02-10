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
[Route(RouteWebVersion1.Secure.BookingConfig.TenantWorkingHour.Base)]
public class TenantWorkingHourController(
    IBookingConfigurationService bookingConfigurationService,
    ILogger<TenantWorkingHourController> logger)
    : ControllerBase
{
    [HttpGet]
    [PermissionAuthorize(Consts.Permissions.TenantSettingsManage)]
    [Route(RouteWebVersion1.Secure.BookingConfig.TenantWorkingHour.All)]
    public async Task<ApiResult> All(Guid tenantId)
    {
        try
        {
            return await bookingConfigurationService.GetTenantWorkingHoursAsync(tenantId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant working hours for tenant {TenantId}", tenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.TenantSettingsManage)]
    [Route(RouteWebVersion1.Secure.BookingConfig.TenantWorkingHour.Upsert)]
    public async Task<ApiResult> Upsert([FromBody] TenantWorkingHourUpsertRequest request)
    {
        try
        {
            return await bookingConfigurationService.UpsertTenantWorkingHourAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting tenant working hour for tenant {TenantId}", request.TenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.TenantSettingsManage)]
    [Route(RouteWebVersion1.Secure.BookingConfig.TenantWorkingHour.Bulk)]
    public async Task<ApiResult> BulkUpsert(
        [FromBody] List<TenantWorkingHourUpsertRequest> requests,
        [FromQuery] bool allOrNothing = false)
    {
        try
        {
            return await bookingConfigurationService.UpsertTenantWorkingHoursAsync(requests, allOrNothing);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error bulk upserting tenant working hours");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
