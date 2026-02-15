using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Equipment;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Interfaces.Services.Equipment;

public interface IEquipmentManagementService
{
    Task<ApiResult> GetTenantEquipmentAsync(
        List<EquipmentStatus>? statuses = null,
        string? category = null,
        string? search = null,
        string? city = null,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default);

    Task<ApiResult> GetEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken = default);
    Task<ApiResult> CreateEquipmentAsync(EquipmentCreateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> UpdateEquipmentAsync(EquipmentUpdateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> PublishEquipmentAsync(EquipmentPublishRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> UpdateEquipmentStatusAsync(EquipmentStatusUpdateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> DeleteEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken = default);

    Task<ApiResult> UpsertEquipmentPricingRulesAsync(
        Guid equipmentId,
        List<EquipmentPricingRuleUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpsertEquipmentSpecsAsync(
        Guid equipmentId,
        List<EquipmentSpecUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpsertEquipmentPoliciesAsync(
        Guid equipmentId,
        List<EquipmentPolicyUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpsertEquipmentMediaAsync(
        Guid equipmentId,
        List<EquipmentMediaUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default);
}
