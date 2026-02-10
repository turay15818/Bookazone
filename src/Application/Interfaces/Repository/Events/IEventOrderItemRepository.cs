using Bookazone.Domain.Entities.Events;

namespace Bookazone.Application.Interfaces.Repository.Events;

public interface IEventOrderItemRepository : IGenericRepository<EventOrderItem>
{
    Task<List<EventOrderItem>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, int>> GetReservedQuantitiesAsync(
        Guid eventId,
        IReadOnlyCollection<Guid> ticketTypeIds,
        DateTime nowUtc,
        CancellationToken cancellationToken = default);
}
