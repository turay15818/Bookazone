using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Events;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Interfaces.Services.Events;

public interface IEventOrderService
{
    Task<ApiResult> CreateOrderAsync(EventOrderCreateRequest request, CancellationToken cancellationToken = default);

    Task<ApiResult> ConfirmOrderAsync(EventOrderConfirmRequest request, CancellationToken cancellationToken = default);

    Task<ApiResult> CancelOrderAsync(EventOrderCancelRequest request, CancellationToken cancellationToken = default);

    Task<ApiResult> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<ApiResult> GetCustomerOrdersAsync(
        List<EventOrderStatus>? statuses = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        Guid? eventId = null,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default);

    Task<ApiResult> GetTenantOrdersAsync(
        List<EventOrderStatus>? statuses = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        Guid? eventId = null,
        Guid? customerId = null,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default);
}
