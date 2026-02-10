using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Events;
using Bookazone.Application.Interfaces.Services.Events;
using Bookazone.Domain.Enums;

namespace Bookazone.Api.Controllers.V1.Secure.Events;

[Authorize]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.Events.Slug)]
[Route(RouteWebVersion1.Secure.Events.Orders.Base)]
public class EventOrderController(
    IEventOrderService eventOrderService,
    ILogger<EventOrderController> logger)
    : ControllerBase
{
    [HttpPost]
    [Route(RouteWebVersion1.Secure.Events.Orders.Create)]
    public async Task<ApiResult> Create([FromBody] EventOrderCreateRequest request)
    {
        try
        {
            return await eventOrderService.CreateOrderAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating event order");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.Events.Orders.Confirm)]
    public async Task<ApiResult> Confirm([FromBody] EventOrderConfirmRequest request)
    {
        try
        {
            return await eventOrderService.ConfirmOrderAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error confirming event order {OrderId}", request.OrderId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.Events.Orders.Cancel)]
    public async Task<ApiResult> Cancel([FromBody] EventOrderCancelRequest request)
    {
        try
        {
            return await eventOrderService.CancelOrderAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cancelling event order {OrderId}", request.OrderId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Secure.Events.Orders.Find)]
    public async Task<ApiResult> Find(Guid id)
    {
        try
        {
            return await eventOrderService.GetOrderAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error finding event order {OrderId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Secure.Events.Orders.Customer)]
    public async Task<ApiResult> Customer(
        [FromQuery] List<EventOrderStatus>? statuses = null,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        [FromQuery] Guid? eventId = null,
        [FromQuery] int? pageIndex = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        try
        {
            return await eventOrderService.GetCustomerOrdersAsync(
                statuses,
                fromUtc,
                toUtc,
                eventId,
                pageIndex,
                pageSize,
                sortBy,
                sortDirection);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting customer event orders");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Secure.Events.Orders.Tenant)]
    public async Task<ApiResult> Tenant(
        [FromQuery] List<EventOrderStatus>? statuses = null,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        [FromQuery] Guid? eventId = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] int? pageIndex = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        try
        {
            return await eventOrderService.GetTenantOrdersAsync(
                statuses,
                fromUtc,
                toUtc,
                eventId,
                customerId,
                pageIndex,
                pageSize,
                sortBy,
                sortDirection);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant event orders");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
