using Bookazone.Application.Interfaces.Repository;
using Bookazone.Domain.Entities.Events;

namespace Bookazone.Application.Interfaces.Repository.Events;

public interface IEventCategoryRepository : IGenericRepository<EventCategory>
{
    Task<EventCategory?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<List<EventCategory>> GetActiveAsync(CancellationToken cancellationToken = default);
}
