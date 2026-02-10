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
[Route(RouteWebVersion1.Secure.BookingConfig.SportResourceAvailability.Base)]
public class SportResourceAvailabilityController(
    IBookingConfigurationService bookingConfigurationService,
    ILogger<SportResourceAvailabilityController> logger)
    : ControllerBase
{
    [HttpGet]
    [PermissionAuthorize(Consts.Permissions.ResourceView)]
    [Route(RouteWebVersion1.Secure.BookingConfig.SportResourceAvailability.All)]
    public async Task<ApiResult> All(Guid resourceId)
    {
        try
        {
            return await bookingConfigurationService.GetSportResourceAvailabilitiesAsync(resourceId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sport resource availability for resource {ResourceId}", resourceId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceAvailabilityManage)]
    [Route(RouteWebVersion1.Secure.BookingConfig.SportResourceAvailability.Upsert)]
    public async Task<ApiResult> Upsert([FromBody] SportResourceAvailabilityUpsertRequest request)
    {
        try
        {
            return await bookingConfigurationService.UpsertSportResourceAvailabilityAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting sport resource availability for resource {ResourceId}", request.ResourceId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceAvailabilityManage)]
    [Route(RouteWebVersion1.Secure.BookingConfig.SportResourceAvailability.Bulk)]
    public async Task<ApiResult> BulkUpsert(
        [FromBody] List<SportResourceAvailabilityUpsertRequest> requests,
        [FromQuery] bool allOrNothing = false)
    {
        try
        {
            return await bookingConfigurationService.UpsertSportResourceAvailabilitiesAsync(requests, allOrNothing);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error bulk upserting sport resource availabilities");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
