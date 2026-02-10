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
[Route(RouteWebVersion1.Secure.BookingConfig.SportResourceBlackout.Base)]
public class SportResourceBlackoutController(
    IBookingConfigurationService bookingConfigurationService,
    ILogger<SportResourceBlackoutController> logger)
    : ControllerBase
{
    [HttpGet]
    [PermissionAuthorize(Consts.Permissions.ResourceView)]
    [Route(RouteWebVersion1.Secure.BookingConfig.SportResourceBlackout.All)]
    public async Task<ApiResult> All(Guid resourceId, DateTime? fromUtc = null, DateTime? toUtc = null)
    {
        try
        {
            return await bookingConfigurationService.GetSportResourceBlackoutsAsync(resourceId, fromUtc, toUtc);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sport resource blackouts for resource {ResourceId}", resourceId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceAvailabilityManage)]
    [Route(RouteWebVersion1.Secure.BookingConfig.SportResourceBlackout.Create)]
    public async Task<ApiResult> Create([FromBody] SportResourceBlackoutCreateRequest request)
    {
        try
        {
            return await bookingConfigurationService.CreateSportResourceBlackoutAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating sport resource blackout for resource {ResourceId}", request.ResourceId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpDelete]
    [PermissionAuthorize(Consts.Permissions.ResourceAvailabilityManage)]
    [Route(RouteWebVersion1.Secure.BookingConfig.SportResourceBlackout.Delete)]
    public async Task<ApiResult> Delete(Guid id)
    {
        try
        {
            return await bookingConfigurationService.DeleteSportResourceBlackoutAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting sport resource blackout {BlackoutId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
