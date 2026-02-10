using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Events;
using Bookazone.Domain.Entities.Events;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Events;

public class EventRepository(
    BookazoneDbContext context,
    ILogger<EventRepository> logger)
    : BaseRepository<Event>(context, logger), IEventRepository
{
    public async Task<List<Event>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await Context.Events
            .AsNoTracking()
            .Where(e =>
                e.FkTenantId == tenantId &&
                e.Active &&
                !e.Deleted)
            .OrderByDescending(e => e.StartUtc)
            .ToListAsync(cancellationToken);
    }
}
