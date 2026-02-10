using Bookazone.Application.Interfaces.Repository;
using Bookazone.Domain.Entities.Events;

namespace Bookazone.Application.Interfaces.Repository.Events;

public interface IEventPolicyRepository : IGenericRepository<EventPolicy>
{
    Task<List<EventPolicy>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default);
}
