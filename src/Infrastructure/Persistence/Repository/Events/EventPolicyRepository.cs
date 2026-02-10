using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Events;
using Bookazone.Domain.Entities.Events;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Events;

public class EventPolicyRepository(
    BookazoneDbContext context,
    ILogger<EventPolicyRepository> logger)
    : BaseRepository<EventPolicy>(context, logger), IEventPolicyRepository
{
    public async Task<List<EventPolicy>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await Context.EventPolicies
            .AsNoTracking()
            .Where(p =>
                p.FkEventId == eventId &&
                p.Active &&
                !p.Deleted)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(cancellationToken);
    }
}
