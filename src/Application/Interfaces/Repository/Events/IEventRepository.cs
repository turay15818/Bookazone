using Bookazone.Application.Interfaces.Repository;
using Bookazone.Domain.Entities.Events;

namespace Bookazone.Application.Interfaces.Repository.Events;

public interface IEventRepository : IGenericRepository<Event>
{
    Task<List<Event>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
