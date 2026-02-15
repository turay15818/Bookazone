using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Rentals;

namespace Bookazone.Application.Interfaces.Services.Rentals;

public interface IRentalDiscoveryService
{
    Task<ApiResult> GetRentalsAsync(RentalSearchRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> GetRentalAsync(Guid rentalId, CancellationToken cancellationToken = default);
    Task<ApiResult> GetRentalSlotsAsync(Guid rentalId, DateTime dateLocal, CancellationToken cancellationToken = default);
}
