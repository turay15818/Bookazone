using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Events;
using Bookazone.Domain.Entities.Events;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Events;

public class EventTicketTypeRepository(
    BookazoneDbContext context,
    ILogger<EventTicketTypeRepository> logger)
    : BaseRepository<EventTicketType>(context, logger), IEventTicketTypeRepository
{
    public async Task<List<EventTicketType>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await Context.EventTicketTypes
            .AsNoTracking()
            .Where(t =>
                t.FkEventId == eventId &&
                t.Active &&
                !t.Deleted)
            .OrderBy(t => t.PriceAmount)
            .ToListAsync(cancellationToken);
    }
}
