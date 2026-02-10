using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Events;
using Bookazone.Domain.Entities.Events;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Events;

public class EventCategorySubscriptionRepository(
    BookazoneDbContext context,
    ILogger<EventCategorySubscriptionRepository> logger)
    : BaseRepository<EventCategorySubscription>(context, logger), IEventCategorySubscriptionRepository
{
    public async Task<EventCategorySubscription?> GetByUserAndCategoryAsync(
        Guid userId,
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        return await Context.EventCategorySubscriptions
            .FirstOrDefaultAsync(s =>
                s.FkUserId == userId &&
                s.FkEventCategoryId == categoryId,
                cancellationToken);
    }

    public async Task<List<EventCategorySubscription>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await Context.EventCategorySubscriptions
            .AsNoTracking()
            .Where(s => s.FkUserId == userId && s.Active && !s.Deleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EventCategorySubscription>> GetActiveByCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        return await Context.EventCategorySubscriptions
            .AsNoTracking()
            .Where(s => s.FkEventCategoryId == categoryId && s.Active && !s.Deleted)
            .ToListAsync(cancellationToken);
    }
}
