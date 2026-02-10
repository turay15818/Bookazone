using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Sports;
using Bookazone.Domain.Entities.Sports;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Sports;

public class SportResourceBlackoutRepository(
    BookazoneDbContext context,
    ILogger<SportResourceBlackoutRepository> logger)
    : BaseRepository<SportResourceBlackout>(context, logger), ISportResourceBlackoutRepository
{
    public async Task<List<SportResourceBlackout>> GetByResourceAsync(
        Guid resourceId,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.SportResourceBlackouts
            .AsNoTracking()
            .Where(b =>
                b.FkSportResourceId == resourceId &&
                b.Active &&
                !b.Deleted);

        query = ApplyRangeFilter(query, fromUtc, toUtc);

        return await query
            .OrderBy(b => b.StartUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasOverlapAsync(
        Guid resourceId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default)
    {
        if (endUtc <= startUtc)
            throw new ArgumentException("EndUtc must be greater than StartUtc.", nameof(endUtc));

        return await Context.SportResourceBlackouts
            .AsNoTracking()
            .Where(b =>
                b.FkSportResourceId == resourceId &&
                b.Active &&
                !b.Deleted &&
                b.StartUtc < endUtc &&
                b.EndUtc > startUtc)
            .AnyAsync(cancellationToken);
    }

    private static IQueryable<SportResourceBlackout> ApplyRangeFilter(
        IQueryable<SportResourceBlackout> query,
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
