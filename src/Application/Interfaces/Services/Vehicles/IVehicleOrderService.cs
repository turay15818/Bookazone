using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Vehicles;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Interfaces.Services.Vehicles;

public interface IVehicleOrderService
{
    Task<ApiResult> CreateAsync(VehicleOrderCreateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<ApiResult> GetCustomerOrdersAsync(
        List<VehicleOrderStatus>? statuses = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        Guid? tenantId = null,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default);
    Task<ApiResult> GetTenantOrdersAsync(
        List<VehicleOrderStatus>? statuses = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        Guid? vehicleId = null,
        Guid? customerId = null,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default);
    Task<ApiResult> UpdateStatusAsync(VehicleOrderStatusUpdateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> CancelAsync(VehicleOrderCancelRequest request, CancellationToken cancellationToken = default);
}
