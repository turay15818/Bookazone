using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Booking;
using Bookazone.Domain.Enums;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;
using BookingEntity = Bookazone.Domain.Entities.Booking.Booking;

namespace Bookazone.Infrastructure.Persistence.Repository.Booking;

public class BookingRepository(
    BookazoneDbContext context,
    ILogger<BookingRepository> logger)
    : BaseRepository<BookingEntity>(context, logger), IBookingRepository
{
    public async Task<List<BookingEntity>> GetByTenantAsync(
        Guid tenantId,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Bookings
            .AsNoTracking()
            .Where(b => b.FkTenantId == tenantId && b.Active && !b.Deleted);

        query = ApplyRangeFilter(query, fromUtc, toUtc);

        return await query
            .OrderByDescending(b => b.StartUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<BookingEntity>> GetByResourceAsync(
        Guid resourceId,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Bookings
            .AsNoTracking()
            .Where(b => b.FkSportResourceId == resourceId && b.Active && !b.Deleted);

        query = ApplyRangeFilter(query, fromUtc, toUtc);

        return await query
            .OrderByDescending(b => b.StartUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasOverlapAsync(
        Guid resourceId,
        DateTime startUtc,
        DateTime endUtc,
        Guid? excludeBookingId = null,
        CancellationToken cancellationToken = default)
    {
        if (endUtc <= startUtc)
            throw new ArgumentException("EndUtc must be greater than StartUtc.", nameof(endUtc));

        return await Context.Bookings
            .AsNoTracking()
            .Where(b =>
                b.FkSportResourceId == resourceId &&
                b.Active &&
                !b.Deleted &&
                b.Status != BookingStatus.Cancelled &&
                b.Status != BookingStatus.Declined &&
                (!excludeBookingId.HasValue || b.Id != excludeBookingId.Value) &&
                b.StartUtc < endUtc &&
                b.EndUtc > startUtc)
            .AnyAsync(cancellationToken);
    }

    private static IQueryable<BookingEntity> ApplyRangeFilter(
        IQueryable<BookingEntity> query,
        DateTime? fromUtc,
        DateTime? toUtc)
    {
        if (fromUtc.HasValue)
            query = query.Where(b => b.EndUtc > fromUtc.Value);

        if (toUtc.HasValue)
            query = query.Where(b => b.StartUtc < toUtc.Value);

        return query;
    }
}
