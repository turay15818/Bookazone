using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Vehicles;
using Bookazone.Application.Interfaces.Services.Vehicles;
using Bookazone.Domain.Enums;

namespace Bookazone.Api.Controllers.V1.Public;

[AllowAnonymous]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.Vehicles.Slug)]
[Route(RouteWebVersion1.Public.Vehicles.Base)]
public class VehiclesController(
    IVehicleDiscoveryService vehicleDiscoveryService,
    ILogger<VehiclesController> logger)
    : ControllerBase
{
    [HttpGet]
    [Route(RouteWebVersion1.Public.Vehicles.All)]
    public async Task<ApiResult> All(
        [FromQuery] Guid? tenantId = null,
        [FromQuery] VehicleServiceType? serviceType = null,
        [FromQuery] VehicleType? vehicleType = null,
        [FromQuery] string? city = null,
        [FromQuery] string? search = null,
        [FromQuery] int? minSeats = null,
        [FromQuery] VehicleTransmissionType? transmission = null,
        [FromQuery] VehicleFuelType? fuelType = null,
        [FromQuery] int? pageIndex = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        try
        {
            var request = new VehicleSearchRequest
            {
                TenantId = tenantId,
                ServiceType = serviceType,
                VehicleType = vehicleType,
                City = city,
                Search = search,
                MinSeats = minSeats,
                Transmission = transmission,
                FuelType = fuelType,
                PageIndex = pageIndex,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

            return await vehicleDiscoveryService.GetVehiclesAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting vehicles");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Public.Vehicles.Find)]
    public async Task<ApiResult> Find([FromRoute] Guid id)
    {
        try
        {
            return await vehicleDiscoveryService.GetVehicleAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting vehicle {VehicleId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
