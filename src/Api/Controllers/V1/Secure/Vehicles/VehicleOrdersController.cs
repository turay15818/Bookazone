using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Vehicles;
using Bookazone.Application.Interfaces.Services.Vehicles;
using Bookazone.Domain.Enums;

namespace Bookazone.Api.Controllers.V1.Secure.Vehicles;

[Authorize]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.Vehicles.Slug)]
[Route(RouteWebVersion1.Secure.VehicleOrders.Base)]
public class VehicleOrdersController(
    IVehicleOrderService vehicleOrderService,
    ILogger<VehicleOrdersController> logger)
    : ControllerBase
{
    [HttpPost]
    [Route(RouteWebVersion1.Secure.VehicleOrders.Create)]
    public async Task<ApiResult> Create([FromBody] VehicleOrderCreateRequest request)
    {
        try
        {
            return await vehicleOrderService.CreateAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating vehicle order for vehicle {VehicleId}", request.VehicleId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Secure.VehicleOrders.Find)]
    public async Task<ApiResult> Find([FromQuery] Guid id)
    {
        try
        {
            return await vehicleOrderService.GetOrderAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting vehicle order {OrderId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Secure.VehicleOrders.Customer)]
    public async Task<ApiResult> Customer(
        [FromQuery] List<VehicleOrderStatus>? statuses = null,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        [FromQuery] Guid? tenantId = null,
        [FromQuery] int? pageIndex = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        try
        {
            return await vehicleOrderService.GetCustomerOrdersAsync(
                statuses,
                fromUtc,
                toUtc,
                tenantId,
                pageIndex,
                pageSize,
                sortBy,
                sortDirection);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting customer vehicle orders");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Secure.VehicleOrders.Tenant)]
    public async Task<ApiResult> Tenant(
        [FromQuery] List<VehicleOrderStatus>? statuses = null,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        [FromQuery] Guid? vehicleId = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] int? pageIndex = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        try
        {
            return await vehicleOrderService.GetTenantOrdersAsync(
                statuses,
                fromUtc,
                toUtc,
                vehicleId,
                customerId,
                pageIndex,
                pageSize,
                sortBy,
                sortDirection);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant vehicle orders");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.VehicleOrders.Status)]
    public async Task<ApiResult> UpdateStatus([FromBody] VehicleOrderStatusUpdateRequest request)
    {
        try
        {
            return await vehicleOrderService.UpdateStatusAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating vehicle order status {OrderId}", request.OrderId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.VehicleOrders.Cancel)]
    public async Task<ApiResult> Cancel([FromBody] VehicleOrderCancelRequest request)
    {
        try
        {
            return await vehicleOrderService.CancelAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cancelling vehicle order {OrderId}", request.OrderId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
