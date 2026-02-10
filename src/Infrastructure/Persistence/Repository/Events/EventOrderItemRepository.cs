using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Events;
using Bookazone.Domain.Entities.Events;
using Bookazone.Domain.Enums;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Events;

public class EventOrderItemRepository(
    BookazoneDbContext context,
    ILogger<EventOrderItemRepository> logger)
    : BaseRepository<EventOrderItem>(context, logger), IEventOrderItemRepository
{
    public async Task<List<EventOrderItem>> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await Context.EventOrderItems
            .AsNoTracking()
            .Where(i =>
                i.FkEventOrderId == orderId &&
                i.Active &&
                !i.Deleted)
            .OrderBy(i => i.TicketName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, int>> GetReservedQuantitiesAsync(
        Guid eventId,
        IReadOnlyCollection<Guid> ticketTypeIds,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        if (ticketTypeIds.Count == 0)
            return new Dictionary<Guid, int>();

        var rows = await Context.EventOrderItems
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(i =>
                ticketTypeIds.Contains(i.FkEventTicketTypeId) &&
                i.Active &&
                !i.Deleted &&
                i.FkEventOrder.Active &&
                !i.FkEventOrder.Deleted &&
                i.FkEventOrder.FkEventId == eventId &&
                (i.FkEventOrder.Status == EventOrderStatus.Confirmed ||
                 (i.FkEventOrder.Status == EventOrderStatus.PendingPayment &&
                  i.FkEventOrder.ExpiresAtUtc.HasValue &&
                  i.FkEventOrder.ExpiresAtUtc.Value > nowUtc)))
            .Select(i => new { i.FkEventTicketTypeId, i.Quantity })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(x => x.FkEventTicketTypeId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));
    }
}
