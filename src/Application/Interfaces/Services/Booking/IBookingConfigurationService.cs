using Bookazone.Api.Controllers.V1.Config.Request;
using Bookazone.Application.Common.Shared.Utils;

namespace Bookazone.Application.Interfaces.Services.Booking;

public interface IBookingConfigurationService
{
    Task<ApiResult> GetBookingCategoriesAsync(CancellationToken cancellationToken = default);
    Task<ApiResult> GetBookingCategoryAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResult> CreateBookingCategoryAsync(BookingCategoryCreateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> UpdateBookingCategoryAsync(BookingCategoryUpdateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> DeleteBookingCategoryAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResult> GetTenantBookingCategoriesAsync(Guid tenantId, bool enabledOnly = false, CancellationToken cancellationToken = default);
    Task<ApiResult> UpsertTenantBookingCategoryAsync(TenantBookingCategoryUpsertRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> UpsertTenantBookingCategoriesAsync(List<TenantBookingCategoryUpsertRequest> requests, bool allOrNothing = false, CancellationToken cancellationToken = default);

    Task<ApiResult> GetTenantWorkingHoursAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<ApiResult> UpsertTenantWorkingHourAsync(TenantWorkingHourUpsertRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> UpsertTenantWorkingHoursAsync(List<TenantWorkingHourUpsertRequest> requests, bool allOrNothing = false, CancellationToken cancellationToken = default);

    Task<ApiResult> GetTenantSportTypesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<ApiResult> CreateTenantSportTypeAsync(TenantSportTypeCreateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> UpdateTenantSportTypeAsync(TenantSportTypeUpdateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> DeleteTenantSportTypeAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResult> GetSportResourcesByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<ApiResult> GetSportResourcesByTypeAsync(Guid tenantSportTypeId, CancellationToken cancellationToken = default);
    Task<ApiResult> CreateSportResourceAsync(SportResourceCreateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> UpdateSportResourceAsync(SportResourceUpdateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> DeleteSportResourceAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResult> GetSportResourceAvailabilitiesAsync(Guid resourceId, CancellationToken cancellationToken = default);
    Task<ApiResult> UpsertSportResourceAvailabilityAsync(SportResourceAvailabilityUpsertRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> UpsertSportResourceAvailabilitiesAsync(List<SportResourceAvailabilityUpsertRequest> requests, bool allOrNothing = false, CancellationToken cancellationToken = default);

    Task<ApiResult> GetSportResourceBlackoutsAsync(Guid resourceId, DateTime? fromUtc = null, DateTime? toUtc = null, CancellationToken cancellationToken = default);
    Task<ApiResult> CreateSportResourceBlackoutAsync(SportResourceBlackoutCreateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> DeleteSportResourceBlackoutAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResult> GetSportResourcePricingRulesAsync(Guid resourceId, CancellationToken cancellationToken = default);
    Task<ApiResult> CreateSportResourcePricingRuleAsync(SportResourcePricingRuleCreateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> UpdateSportResourcePricingRuleAsync(SportResourcePricingRuleUpdateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> DeleteSportResourcePricingRuleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResult> UpsertSportResourcePricingRulesAsync(List<SportResourcePricingRuleUpsertRequest> requests, bool allOrNothing = false, CancellationToken cancellationToken = default);
}
