using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Booking;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Interfaces.Services.Booking;

public interface IBookingDiscoveryService
{
    Task<ApiResult> GetSportTypesAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<ApiResult> GetTenantsAsync(BookingCategoryType? category = null, CancellationToken cancellationToken = default);
    Task<ApiResult> SearchSportResourcesAsync(SportResourceSearchRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> GetSportResourceAsync(Guid resourceId, CancellationToken cancellationToken = default);
    Task<ApiResult> GetSportResourceAvailabilityAsync(Guid resourceId, CancellationToken cancellationToken = default);
    Task<ApiResult> GetSportResourceSlotsAsync(Guid resourceId, DateTime dateLocal, CancellationToken cancellationToken = default);
}
