using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Events;
using Bookazone.Domain.Entities.Events;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Events;

public class EventScheduleItemRepository(
    BookazoneDbContext context,
    ILogger<EventScheduleItemRepository> logger)
    : BaseRepository<EventScheduleItem>(context, logger), IEventScheduleItemRepository
{
    public async Task<List<EventScheduleItem>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await Context.EventScheduleItems
            .AsNoTracking()
            .Where(s =>
                s.FkEventId == eventId &&
                s.Active &&
                !s.Deleted)
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.StartUtc)
            .ToListAsync(cancellationToken);
    }
}
