using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Events;
using Bookazone.Domain.Entities.Events;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Events;

public class EventCategoryRepository(
    BookazoneDbContext context,
    ILogger<EventCategoryRepository> logger)
    : BaseRepository<EventCategory>(context, logger), IEventCategoryRepository
{
    public async Task<EventCategory?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var normalized = code.Trim().ToLowerInvariant();
        return await Context.EventCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.Code.ToLower() == normalized &&
                c.Active &&
                !c.Deleted,
                cancellationToken);
    }

    public async Task<List<EventCategory>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await Context.EventCategories
            .AsNoTracking()
            .Where(c => c.Active && !c.Deleted)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}
