using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Vehicles;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Interfaces.Services.Vehicles;

public interface IVehicleManagementService
{
    Task<ApiResult> GetTenantVehiclesAsync(
        List<VehicleStatus>? statuses = null,
        VehicleServiceType? serviceType = null,
        VehicleType? vehicleType = null,
        string? search = null,
        string? city = null,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default);

    Task<ApiResult> GetVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<ApiResult> CreateVehicleAsync(VehicleCreateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> UpdateVehicleAsync(VehicleUpdateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> PublishVehicleAsync(VehiclePublishRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> UpdateVehicleStatusAsync(VehicleStatusUpdateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> DeleteVehicleAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResult> UpsertVehiclePricingRulesAsync(
        Guid vehicleId,
        List<VehiclePricingRuleUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpsertVehicleSpecsAsync(
        Guid vehicleId,
        List<VehicleSpecUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpsertVehiclePoliciesAsync(
        Guid vehicleId,
        List<VehiclePolicyUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpsertVehicleMediaAsync(
        Guid vehicleId,
        List<VehicleMediaUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default);
}
