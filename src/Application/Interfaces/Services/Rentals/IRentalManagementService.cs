using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Rentals;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Interfaces.Services.Rentals;

public interface IRentalManagementService
{
    Task<ApiResult> GetTenantRentalsAsync(
        List<RentalStatus>? statuses = null,
        RentalType? type = null,
        string? search = null,
        string? city = null,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default);

    Task<ApiResult> GetRentalAsync(Guid rentalId, CancellationToken cancellationToken = default);
    Task<ApiResult> CreateRentalAsync(RentalCreateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> UpdateRentalAsync(RentalUpdateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> PublishRentalAsync(RentalPublishRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> UpdateRentalStatusAsync(RentalStatusUpdateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> DeleteRentalAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResult> UpsertRentalPricingRulesAsync(
        Guid rentalId,
        List<RentalPricingRuleUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpsertRentalSpecsAsync(
        Guid rentalId,
        List<RentalSpecUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpsertRentalPoliciesAsync(
        Guid rentalId,
        List<RentalPolicyUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpsertRentalMediaAsync(
        Guid rentalId,
        List<RentalMediaUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default);
}
