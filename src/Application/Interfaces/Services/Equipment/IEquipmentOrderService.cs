using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Equipment;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Interfaces.Services.Equipment;

public interface IEquipmentOrderService
{
    Task<ApiResult> CreateAsync(EquipmentOrderCreateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<ApiResult> GetCustomerOrdersAsync(
        List<EquipmentOrderStatus>? statuses = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        Guid? tenantId = null,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default);
    Task<ApiResult> GetTenantOrdersAsync(
        List<EquipmentOrderStatus>? statuses = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        Guid? equipmentId = null,
        Guid? customerId = null,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default);
    Task<ApiResult> UpdateStatusAsync(EquipmentOrderStatusUpdateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> CancelAsync(EquipmentOrderCancelRequest request, CancellationToken cancellationToken = default);
}
