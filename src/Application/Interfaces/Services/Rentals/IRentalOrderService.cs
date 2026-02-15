using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Rentals;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Interfaces.Services.Rentals;

public interface IRentalOrderService
{
    Task<ApiResult> CreateAsync(RentalOrderCreateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<ApiResult> GetCustomerOrdersAsync(
        List<RentalOrderStatus>? statuses = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        Guid? tenantId = null,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default);
    Task<ApiResult> GetTenantOrdersAsync(
        List<RentalOrderStatus>? statuses = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        Guid? rentalId = null,
        Guid? customerId = null,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default);
    Task<ApiResult> UpdateStatusAsync(RentalOrderStatusUpdateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> CancelAsync(RentalOrderCancelRequest request, CancellationToken cancellationToken = default);
}
