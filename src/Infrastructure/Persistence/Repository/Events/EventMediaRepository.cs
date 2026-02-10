using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Events;
using Bookazone.Domain.Entities.Events;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Events;

public class EventMediaRepository(
    BookazoneDbContext context,
    ILogger<EventMediaRepository> logger)
    : BaseRepository<EventMedia>(context, logger), IEventMediaRepository
{
    public async Task<List<EventMedia>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await Context.EventMedias
            .AsNoTracking()
            .Where(m =>
                m.FkEventId == eventId &&
                m.Active &&
                !m.Deleted)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);
    }
}
