using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Rentals;
using Bookazone.Application.Interfaces.Services.Rentals;
using Bookazone.Domain.Enums;

namespace Bookazone.Api.Controllers.V1.Public;

[AllowAnonymous]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.Rentals.Slug)]
[Route(RouteWebVersion1.Public.Rentals.Base)]
public class RentalsController(
    IRentalDiscoveryService rentalDiscoveryService,
    ILogger<RentalsController> logger)
    : ControllerBase
{
    [HttpGet]
    [Route(RouteWebVersion1.Public.Rentals.All)]
    public async Task<ApiResult> All(
        [FromQuery] Guid? tenantId = null,
        [FromQuery] RentalType? type = null,
        [FromQuery] string? city = null,
        [FromQuery] string? search = null,
        [FromQuery] int? minCapacity = null,
        [FromQuery] int? minBedrooms = null,
        [FromQuery] int? minBathrooms = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] int? pageIndex = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        try
        {
            var request = new RentalSearchRequest
            {
                TenantId = tenantId,
                Type = type,
                City = city,
                Search = search,
                MinCapacity = minCapacity,
                MinBedrooms = minBedrooms,
                MinBathrooms = minBathrooms,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                PageIndex = pageIndex,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

            return await rentalDiscoveryService.GetRentalsAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting rentals");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Public.Rentals.Find)]
    public async Task<ApiResult> Find([FromRoute] Guid id)
    {
        try
        {
            return await rentalDiscoveryService.GetRentalAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting rental {RentalId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Public.Rentals.Slots)]
    public async Task<ApiResult> Slots([FromRoute] Guid id, [FromQuery] DateTime dateLocal)
    {
        if (dateLocal == default)
            return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "dateLocal is required.");

        try
        {
            return await rentalDiscoveryService.GetRentalSlotsAsync(id, dateLocal);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting rental slots {RentalId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
