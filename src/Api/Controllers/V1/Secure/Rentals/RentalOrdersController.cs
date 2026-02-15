using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Rentals;
using Bookazone.Application.Interfaces.Services.Rentals;
using Bookazone.Domain.Enums;

namespace Bookazone.Api.Controllers.V1.Secure.Rentals;

[Authorize]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.Rentals.Slug)]
[Route(RouteWebVersion1.Secure.RentalOrders.Base)]
public class RentalOrdersController(
    IRentalOrderService rentalOrderService,
    ILogger<RentalOrdersController> logger)
    : ControllerBase
{
    [HttpPost]
    [Route(RouteWebVersion1.Secure.RentalOrders.Create)]
    public async Task<ApiResult> Create([FromBody] RentalOrderCreateRequest request)
    {
        try
        {
            return await rentalOrderService.CreateAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating rental order for rental {RentalId}", request.RentalId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Secure.RentalOrders.Find)]
    public async Task<ApiResult> Find([FromQuery] Guid id)
    {
        try
        {
            return await rentalOrderService.GetOrderAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting rental order {OrderId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Secure.RentalOrders.Customer)]
    public async Task<ApiResult> Customer(
        [FromQuery] List<RentalOrderStatus>? statuses = null,
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
            return await rentalOrderService.GetCustomerOrdersAsync(
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
            logger.LogError(ex, "Error getting customer rental orders");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Secure.RentalOrders.Tenant)]
    public async Task<ApiResult> Tenant(
        [FromQuery] List<RentalOrderStatus>? statuses = null,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        [FromQuery] Guid? rentalId = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] int? pageIndex = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        try
        {
            return await rentalOrderService.GetTenantOrdersAsync(
                statuses,
                fromUtc,
                toUtc,
                rentalId,
                customerId,
                pageIndex,
                pageSize,
                sortBy,
                sortDirection);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant rental orders");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.RentalOrders.Status)]
    public async Task<ApiResult> UpdateStatus([FromBody] RentalOrderStatusUpdateRequest request)
    {
        try
        {
            return await rentalOrderService.UpdateStatusAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating rental order status {OrderId}", request.OrderId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.RentalOrders.Cancel)]
    public async Task<ApiResult> Cancel([FromBody] RentalOrderCancelRequest request)
    {
        try
        {
            return await rentalOrderService.CancelAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cancelling rental order {OrderId}", request.OrderId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
