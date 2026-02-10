using Bookazone.Application.Interfaces.Repository;
using Bookazone.Domain.Entities.Events;

namespace Bookazone.Application.Interfaces.Repository.Events;

public interface IEventMediaRepository : IGenericRepository<EventMedia>
{
    Task<List<EventMedia>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default);
}
