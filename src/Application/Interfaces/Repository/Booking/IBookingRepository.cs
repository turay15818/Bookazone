using Bookazone.Application.Interfaces.Repository;
using BookingEntity = Bookazone.Domain.Entities.Booking.Booking;

namespace Bookazone.Application.Interfaces.Repository.Booking;

public interface IBookingRepository : IGenericRepository<BookingEntity>
{
    Task<List<BookingEntity>> GetByTenantAsync(
        Guid tenantId,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default);

    Task<List<BookingEntity>> GetByResourceAsync(
        Guid resourceId,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default);

    Task<bool> HasOverlapAsync(
        Guid resourceId,
        DateTime startUtc,
        DateTime endUtc,
        Guid? excludeBookingId = null,
        CancellationToken cancellationToken = default);
}
