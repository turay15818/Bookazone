using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Booking;
using Bookazone.Application.Interfaces.Services.Booking;

namespace Bookazone.Api.Controllers.V1.Public;

[AllowAnonymous]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.Booking.Slug)]
[Route(RouteWebVersion1.Public.Booking.Sports.Base)]
public class BookingSportsController(
    IBookingDiscoveryService bookingDiscoveryService,
    ILogger<BookingSportsController> logger)
    : ControllerBase
{
    [HttpGet]
    [Route(RouteWebVersion1.Public.Booking.Sports.Types)]
    public async Task<ApiResult> Types([FromQuery] Guid? tenantId = null)
    {
        try
        {
            return await bookingDiscoveryService.GetSportTypesAsync(tenantId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sport types");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Public.Booking.Sports.Resources)]
    public async Task<ApiResult> Resources(
        [FromQuery] Guid? tenantId = null,
        [FromQuery] Guid? sportTypeId = null,
        [FromQuery] int? minCapacity = null,
        [FromQuery] string? search = null,
        [FromQuery] string? city = null)
    {
        try
        {
            var request = new SportResourceSearchRequest
            {
                TenantId = tenantId,
                SportTypeId = sportTypeId,
                MinCapacity = minCapacity,
                Search = search,
                City = city
            };

            return await bookingDiscoveryService.SearchSportResourcesAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching sport resources");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Public.Booking.Sports.Resource)]
    public async Task<ApiResult> Resource([FromRoute] Guid id)
    {
        try
        {
            return await bookingDiscoveryService.GetSportResourceAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sport resource {ResourceId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Public.Booking.Sports.Availability)]
    public async Task<ApiResult> Availability([FromRoute] Guid id)
    {
        try
        {
            return await bookingDiscoveryService.GetSportResourceAvailabilityAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sport resource availability {ResourceId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Public.Booking.Sports.Slots)]
    public async Task<ApiResult> Slots([FromRoute] Guid id, [FromQuery] DateTime dateLocal)
    {
        if (dateLocal == default)
            return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "dateLocal is required.");

        try
        {
            return await bookingDiscoveryService.GetSportResourceSlotsAsync(id, dateLocal);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sport resource slots {ResourceId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
