using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Equipment;
using Bookazone.Application.Interfaces.Services.Equipment;
using Bookazone.Domain.Enums;

namespace Bookazone.Api.Controllers.V1.Secure.Equipment;

[Authorize]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.Equipment.Slug)]
[Route(RouteWebVersion1.Secure.EquipmentOrders.Base)]
public class EquipmentOrdersController(
    IEquipmentOrderService equipmentOrderService,
    ILogger<EquipmentOrdersController> logger)
    : ControllerBase
{
    [HttpPost]
    [Route(RouteWebVersion1.Secure.EquipmentOrders.Create)]
    public async Task<ApiResult> Create([FromBody] EquipmentOrderCreateRequest request)
    {
        try
        {
            return await equipmentOrderService.CreateAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating equipment order for equipment {EquipmentId}", request.EquipmentId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Secure.EquipmentOrders.Find)]
    public async Task<ApiResult> Find([FromQuery] Guid id)
    {
        try
        {
            return await equipmentOrderService.GetOrderAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting equipment order {OrderId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Secure.EquipmentOrders.Customer)]
    public async Task<ApiResult> Customer(
        [FromQuery] List<EquipmentOrderStatus>? statuses = null,
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
            return await equipmentOrderService.GetCustomerOrdersAsync(
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
            logger.LogError(ex, "Error getting customer equipment orders");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Secure.EquipmentOrders.Tenant)]
    public async Task<ApiResult> Tenant(
        [FromQuery] List<EquipmentOrderStatus>? statuses = null,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        [FromQuery] Guid? equipmentId = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] int? pageIndex = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        try
        {
            return await equipmentOrderService.GetTenantOrdersAsync(
                statuses,
                fromUtc,
                toUtc,
                equipmentId,
                customerId,
                pageIndex,
                pageSize,
                sortBy,
                sortDirection);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant equipment orders");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.EquipmentOrders.Status)]
    public async Task<ApiResult> UpdateStatus([FromBody] EquipmentOrderStatusUpdateRequest request)
    {
        try
        {
            return await equipmentOrderService.UpdateStatusAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating equipment order status {OrderId}", request.OrderId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.EquipmentOrders.Cancel)]
    public async Task<ApiResult> Cancel([FromBody] EquipmentOrderCancelRequest request)
    {
        try
        {
            return await equipmentOrderService.CancelAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cancelling equipment order {OrderId}", request.OrderId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
