using Bookazone.Application.Interfaces.Repository;
using Bookazone.Domain.Entities.Events;

namespace Bookazone.Application.Interfaces.Repository.Events;

public interface IEventTicketTypeRepository : IGenericRepository<EventTicketType>
{
    Task<List<EventTicketType>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default);
}
