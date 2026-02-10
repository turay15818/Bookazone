using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Services.Booking;
using Bookazone.Domain.Enums;

namespace Bookazone.Api.Controllers.V1.Public;

[AllowAnonymous]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.Booking.Slug)]
[Route(RouteWebVersion1.Public.Booking.Tenants.Base)]
public class BookingTenantsController(
    IBookingDiscoveryService bookingDiscoveryService,
    ILogger<BookingTenantsController> logger)
    : ControllerBase
{
    [HttpGet]
    [Route(RouteWebVersion1.Public.Booking.Tenants.All)]
    public async Task<ApiResult> All([FromQuery] BookingCategoryType? category = null)
    {
        try
        {
            return await bookingDiscoveryService.GetTenantsAsync(category);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenants for category {Category}", category);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
