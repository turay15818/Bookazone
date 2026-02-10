using Bookazone.Application.Interfaces.Repository;
using Bookazone.Domain.Entities.Events;

namespace Bookazone.Application.Interfaces.Repository.Events;

public interface IEventCategorySubscriptionRepository : IGenericRepository<EventCategorySubscription>
{
    Task<EventCategorySubscription?> GetByUserAndCategoryAsync(
        Guid userId,
        Guid categoryId,
        CancellationToken cancellationToken = default);

    Task<List<EventCategorySubscription>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<List<EventCategorySubscription>> GetActiveByCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);
}
