using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Equipment;
using Bookazone.Application.Interfaces.Services.Equipment;
using Bookazone.Domain.Enums;

namespace Bookazone.Api.Controllers.V1.Public;

[AllowAnonymous]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.Equipment.Slug)]
[Route(RouteWebVersion1.Public.Equipment.Base)]
public class EquipmentController(
    IEquipmentDiscoveryService equipmentDiscoveryService,
    ILogger<EquipmentController> logger)
    : ControllerBase
{
    [HttpGet]
    [Route(RouteWebVersion1.Public.Equipment.All)]
    public async Task<ApiResult> All(
        [FromQuery] Guid? tenantId = null,
        [FromQuery] string? category = null,
        [FromQuery] string? city = null,
        [FromQuery] string? search = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] int? minUnitsAvailable = null,
        [FromQuery] int? pageIndex = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        try
        {
            var request = new EquipmentSearchRequest
            {
                TenantId = tenantId,
                Category = category,
                City = city,
                Search = search,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                MinUnitsAvailable = minUnitsAvailable,
                PageIndex = pageIndex,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

            return await equipmentDiscoveryService.GetEquipmentsAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting equipment");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Public.Equipment.Find)]
    public async Task<ApiResult> Find([FromRoute] Guid id)
    {
        try
        {
            return await equipmentDiscoveryService.GetEquipmentAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting equipment {EquipmentId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Public.Equipment.Availability)]
    public async Task<ApiResult> Availability(
        [FromRoute] Guid id,
        [FromQuery] DateTime startLocal,
        [FromQuery] DateTime endLocal,
        [FromQuery] EquipmentPricingUnit pricingUnit,
        [FromQuery] int unitsRequested = 1)
    {
        if (startLocal == default || endLocal == default)
            return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "startLocal and endLocal are required.");

        try
        {
            return await equipmentDiscoveryService.GetEquipmentAvailabilityAsync(
                id,
                startLocal,
                endLocal,
                pricingUnit,
                unitsRequested);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting equipment availability {EquipmentId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
