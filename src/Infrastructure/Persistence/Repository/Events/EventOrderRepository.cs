using Bookazone.Application.Interfaces.Repository.Events;
using Bookazone.Domain.Entities.Events;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Events;

public class EventOrderRepository(
    BookazoneDbContext context,
    ILogger<EventOrderRepository> logger)
    : BaseRepository<EventOrder>(context, logger), IEventOrderRepository
{
}
