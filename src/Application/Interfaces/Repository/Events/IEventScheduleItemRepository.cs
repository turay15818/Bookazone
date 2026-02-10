using Bookazone.Application.Interfaces.Repository;
using Bookazone.Domain.Entities.Events;

namespace Bookazone.Application.Interfaces.Repository.Events;

public interface IEventScheduleItemRepository : IGenericRepository<EventScheduleItem>
{
    Task<List<EventScheduleItem>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default);
}
