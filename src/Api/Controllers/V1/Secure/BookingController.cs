using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Booking;
using Bookazone.Application.Interfaces.Services.Booking;
using Bookazone.Domain.Enums;

namespace Bookazone.Api.Controllers.V1.Secure;

[Authorize]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.Booking.Slug)]
[Route(RouteWebVersion1.Secure.Booking.Base)]
public class BookingController(
    IBookingService bookingService,
    IBookingValidationService bookingValidationService,
    ILogger<BookingController> logger)
    : ControllerBase
{
    [HttpPost]
    [Route(RouteWebVersion1.Secure.Booking.Validate)]
    public async Task<ApiResult> Validate([FromBody] BookingValidationRequest request)
    {
        try
        {
            var result = await bookingValidationService.ValidateBookingAsync(request);
            return ApiResponse.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating booking for tenant {TenantId}", request.TenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.Booking.Create)]
    public async Task<ApiResult> Create([FromBody] BookingCreateRequest request)
    {
        try
        {
            return await bookingService.CreateAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating booking for tenant {TenantId}", request.TenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.Booking.Approve)]
    public async Task<ApiResult> Approve([FromBody] BookingDecisionRequest request)
    {
        try
        {
            return await bookingService.ApproveAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error approving booking {BookingId}", request.BookingId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.Booking.Decline)]
    public async Task<ApiResult> Decline([FromBody] BookingDecisionRequest request)
    {
        try
        {
            return await bookingService.DeclineAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error declining booking {BookingId}", request.BookingId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.Booking.Cancel)]
    public async Task<ApiResult> Cancel([FromBody] BookingCancelRequest request)
    {
        try
        {
            return await bookingService.CancelAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cancelling booking {BookingId}", request.BookingId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.Booking.Reschedule)]
    public async Task<ApiResult> Reschedule([FromBody] BookingRescheduleRequest request)
    {
        try
        {
            return await bookingService.RescheduleAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error rescheduling booking {BookingId}", request.BookingId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Secure.Booking.TenantList)]
    public async Task<ApiResult> TenantList(
        [FromQuery] List<BookingStatus>? statuses = null,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        [FromQuery] Guid? resourceId = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] int? pageIndex = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        try
        {
            return await bookingService.GetTenantBookingsAsync(
                statuses,
                fromUtc,
                toUtc,
                resourceId,
                customerId,
                pageIndex,
                pageSize,
                sortBy,
                sortDirection);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant bookings");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Secure.Booking.CustomerList)]
    public async Task<ApiResult> CustomerList(
        [FromQuery] List<BookingStatus>? statuses = null,
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
            return await bookingService.GetCustomerBookingsAsync(
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
            logger.LogError(ex, "Error getting customer bookings");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
