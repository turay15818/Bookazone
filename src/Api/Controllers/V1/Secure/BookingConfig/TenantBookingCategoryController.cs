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
[Route(RouteWebVersion1.Secure.BookingConfig.TenantCategory.Base)]
public class TenantBookingCategoryController(
    IBookingConfigurationService bookingConfigurationService,
    ILogger<TenantBookingCategoryController> logger)
    : ControllerBase
{
    [HttpGet]
    [PermissionAuthorize(Consts.Permissions.TenantSettingsManage)]
    [Route(RouteWebVersion1.Secure.BookingConfig.TenantCategory.All)]
    public async Task<ApiResult> All(Guid tenantId, bool enabledOnly = false)
    {
        try
        {
            return await bookingConfigurationService.GetTenantBookingCategoriesAsync(tenantId, enabledOnly);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant booking categories for tenant {TenantId}", tenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.TenantSettingsManage)]
    [Route(RouteWebVersion1.Secure.BookingConfig.TenantCategory.Upsert)]
    public async Task<ApiResult> Upsert([FromBody] TenantBookingCategoryUpsertRequest request)
    {
        try
        {
            return await bookingConfigurationService.UpsertTenantBookingCategoryAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting tenant booking category for tenant {TenantId}", request.TenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.TenantSettingsManage)]
    [Route(RouteWebVersion1.Secure.BookingConfig.TenantCategory.Bulk)]
    public async Task<ApiResult> BulkUpsert(
        [FromBody] List<TenantBookingCategoryUpsertRequest> requests,
        [FromQuery] bool allOrNothing = false)
    {
        try
        {
            return await bookingConfigurationService.UpsertTenantBookingCategoriesAsync(requests, allOrNothing);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error bulk upserting tenant booking categories");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
