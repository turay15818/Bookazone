using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Booking;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Interfaces.Services.Booking;

public interface IBookingService
{
    Task<ApiResult> CreateAsync(BookingCreateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> UpdateAsync(BookingUpdateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> ApproveAsync(BookingDecisionRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> DeclineAsync(BookingDecisionRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> CancelAsync(BookingCancelRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> RescheduleAsync(BookingRescheduleRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> GetTenantBookingsAsync(
        List<BookingStatus>? statuses = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        Guid? resourceId = null,
        Guid? customerId = null,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default);
    Task<ApiResult> GetCustomerBookingsAsync(
        List<BookingStatus>? statuses = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        Guid? tenantId = null,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default);
}
