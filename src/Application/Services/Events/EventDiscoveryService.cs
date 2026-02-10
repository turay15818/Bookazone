using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs;
using Bookazone.Application.DTOs.Converter;
using Bookazone.Application.DTOs.Events;
using Bookazone.Application.DTOs.Response;
using Bookazone.Application.Interfaces.Repository.Events;
using Bookazone.Application.Interfaces.Services.Events;
using Bookazone.Domain.Entities.Events;

namespace Bookazone.Application.Services.Events;

public class EventDiscoveryService(
    IEventRepository eventRepository,
    IEventCategoryRepository eventCategoryRepository,
    IEventTicketTypeRepository eventTicketTypeRepository,
    IEventScheduleItemRepository eventScheduleItemRepository,
    IEventPolicyRepository eventPolicyRepository,
    IEventMediaRepository eventMediaRepository,
    IEventOrderItemRepository eventOrderItemRepository,
    ILogger<EventDiscoveryService> logger)
    : IEventDiscoveryService
{
    public async Task<ApiResult> GetEventCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await eventCategoryRepository.GetActiveAsync(cancellationToken);
            var dto = categories.Select(EventConverter.ToDto);
            return ApiResponse.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting event categories");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetEventsAsync(EventSearchRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            request ??= new EventSearchRequest();

            var query = eventRepository.Query()
                .AsNoTracking()
                .Include(e => e.FkTenant)
                .Include(e => e.FkEventCategory)
                .Include(e => e.FkVenue)
                .Include(e => e.FkCoverMedia)
                .Where(e =>
                    e.Active &&
                    !e.Deleted &&
                    e.Status == EventStatus.Published);

            if (request.TenantId.HasValue)
                query = query.Where(e => e.FkTenantId == request.TenantId.Value);

            if (request.CategoryId.HasValue)
                query = query.Where(e => e.FkEventCategoryId == request.CategoryId.Value);

            if (!string.IsNullOrWhiteSpace(request.CategoryCode))
            {
                var code = request.CategoryCode.Trim().ToUpperInvariant();
                query = query.Where(e => e.FkEventCategory.Code == code);
            }

            if (!string.IsNullOrWhiteSpace(request.City))
            {
                var city = request.City.Trim().ToLowerInvariant();
                query = query.Where(e => e.City != null && e.City.ToLower().Contains(city));
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = request.Search.Trim().ToLowerInvariant();
                query = query.Where(e =>
                    e.Title.ToLower().Contains(term) ||
                    (e.Subtitle != null && e.Subtitle.ToLower().Contains(term)) ||
                    e.FkTenant.Name.ToLower().Contains(term));
            }

            if (request.FromUtc.HasValue)
            {
                var fromUtc = NormalizeUtc(request.FromUtc.Value);
                query = query.Where(e => e.EndUtc >= fromUtc);
            }

            if (request.ToUtc.HasValue)
            {
                var toUtc = NormalizeUtc(request.ToUtc.Value);
                query = query.Where(e => e.StartUtc <= toUtc);
            }

            var sorted = ApplySorting(query, request.SortBy, request.SortDirection);
            var paged = await Paginate<Event>.CreateAsync(sorted, request.PageIndex, request.PageSize);

            var pageData = paged.Data ?? new List<Event>();
            var eventIds = pageData.Select(e => e.Id).ToList();
            var priceLookup = await BuildPriceLookupAsync(eventIds, cancellationToken);

            var dto = pageData.Select(e =>
            {
                var priceInfo = ResolvePrice(e, priceLookup);
                return EventConverter.ToCardDto(
                    e,
                    e.FkEventCategory,
                    e.FkTenant,
                    e.FkVenue,
                    e.FkCoverMedia,
                    priceInfo.PriceFrom,
                    priceInfo.Currency);
            }).ToList();

            return ApiResponse.Success(dto, pages:paged.Pages, pageIndex:paged.PageIndex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting events");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        try
        {
            var evt = await eventRepository.Query()
                .AsNoTracking()
                .Include(e => e.FkTenant)
                .Include(e => e.FkEventCategory)
                .Include(e => e.FkVenue)
                .Include(e => e.FkCoverMedia)
                .FirstOrDefaultAsync(e =>
                    e.Id == eventId &&
                    e.Active &&
                    !e.Deleted &&
                    e.Status == EventStatus.Published,
                    cancellationToken);

            if (evt == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            var ticketTypes = await eventTicketTypeRepository.GetByEventAsync(eventId, cancellationToken);
            var ticketDtos = await BuildTicketTypeDtosAsync(eventId, ticketTypes, cancellationToken);
            var schedule = await eventScheduleItemRepository.GetByEventAsync(eventId, cancellationToken);
            var policies = await eventPolicyRepository.GetByEventAsync(eventId, cancellationToken);
            var media = await eventMediaRepository.GetByEventAsync(eventId, cancellationToken);

            var priceInfo = ResolvePrice(evt, ticketTypes);

            var detail = EventConverter.ToDetailDto(
                evt,
                evt.FkEventCategory,
                evt.FkTenant,
                evt.FkVenue,
                evt.FkCoverMedia?.Url,
                ticketDtos,
                schedule,
                policies,
                media,
                priceInfo.PriceFrom,
                priceInfo.Currency);

            return ApiResponse.Success(detail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting event {EventId}", eventId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetSimilarEventsAsync(Guid eventId, int? take = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var current = await eventRepository.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == eventId && e.Active && !e.Deleted, cancellationToken);

            if (current == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            var query = eventRepository.Query()
                .AsNoTracking()
                .Include(e => e.FkTenant)
                .Include(e => e.FkEventCategory)
                .Include(e => e.FkVenue)
                .Include(e => e.FkCoverMedia)
                .Where(e =>
                    e.Id != eventId &&
                    e.Active &&
                    !e.Deleted &&
                    e.Status == EventStatus.Published);

            if (current.FkEventCategoryId != Guid.Empty)
                query = query.Where(e => e.FkEventCategoryId == current.FkEventCategoryId);

            if (!string.IsNullOrWhiteSpace(current.City))
            {
                var city = current.City.Trim().ToLowerInvariant();
                query = query.Where(e => e.City != null && e.City.ToLower() == city);
            }

            var size = take is > 0 ? take.Value : 6;
            var events = await query
                .OrderBy(e => e.StartUtc)
                .Take(size)
                .ToListAsync(cancellationToken);

            var eventIds = events.Select(e => e.Id).ToList();
            var priceLookup = await BuildPriceLookupAsync(eventIds, cancellationToken);

            var dto = events.Select(e =>
            {
                var priceInfo = ResolvePrice(e, priceLookup);
                return EventConverter.ToCardDto(
                    e,
                    e.FkEventCategory,
                    e.FkTenant,
                    e.FkVenue,
                    e.FkCoverMedia,
                    priceInfo.PriceFrom,
                    priceInfo.Currency);
            });

            return ApiResponse.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting similar events for {EventId}", eventId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    private static IQueryable<Event> ApplySorting(IQueryable<Event> query, string? sortBy, string? sortDirection)
    {
        var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "title" => isDesc ? query.OrderByDescending(e => e.Title) : query.OrderBy(e => e.Title),
            "endutc" => isDesc ? query.OrderByDescending(e => e.EndUtc) : query.OrderBy(e => e.EndUtc),
            "price" => isDesc ? query.OrderByDescending(e => e.PriceFrom) : query.OrderBy(e => e.PriceFrom),
            "city" => isDesc ? query.OrderByDescending(e => e.City) : query.OrderBy(e => e.City),
            _ => isDesc ? query.OrderByDescending(e => e.StartUtc) : query.OrderBy(e => e.StartUtc)
        };
    }

    private async Task<Dictionary<Guid, PriceInfo>> BuildPriceLookupAsync(
        List<Guid> eventIds,
        CancellationToken cancellationToken)
    {
        if (eventIds.Count == 0)
            return new Dictionary<Guid, PriceInfo>();

        var tickets = await eventTicketTypeRepository.Query()
            .AsNoTracking()
            .Where(t =>
                eventIds.Contains(t.FkEventId) &&
                t.Active &&
                !t.Deleted)
            .Select(t => new { t.FkEventId, t.PriceAmount, t.Currency })
            .ToListAsync(cancellationToken);

        return tickets
            .GroupBy(t => t.FkEventId)
            .ToDictionary(
                g => g.Key,
                g => new PriceInfo(
                    g.Min(x => x.PriceAmount),
                    g.OrderBy(x => x.PriceAmount).Select(x => x.Currency).FirstOrDefault()));
    }

    private static PriceInfo ResolvePrice(Event evt, Dictionary<Guid, PriceInfo> lookup)
    {
        if (!lookup.TryGetValue(evt.Id, out var info))
            info = new PriceInfo(null, null);

        var price = evt.PriceFrom ?? info.PriceFrom;
        return new PriceInfo(price, info.Currency);
    }

    private static PriceInfo ResolvePrice(Event evt, IReadOnlyCollection<EventTicketType> tickets)
    {
        if (tickets.Count == 0)
            return new PriceInfo(evt.PriceFrom, null);

        var cheapest = tickets.OrderBy(t => t.PriceAmount).First();
        var price = evt.PriceFrom ?? cheapest.PriceAmount;
        return new PriceInfo(price, cheapest.Currency);
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    private async Task<List<VwEventTicketType>> BuildTicketTypeDtosAsync(
        Guid eventId,
        List<EventTicketType> ticketTypes,
        CancellationToken cancellationToken)
    {
        if (ticketTypes.Count == 0)
            return new List<VwEventTicketType>();

        var nowUtc = DateTime.UtcNow;
        var ticketTypeIds = ticketTypes.Select(t => t.Id).ToList();
        var reservedLookup = await eventOrderItemRepository.GetReservedQuantitiesAsync(
            eventId,
            ticketTypeIds,
            nowUtc,
            cancellationToken);

        return ticketTypes
            .Select(t => EventConverter.ToDto(t, ResolveAvailableQuantity(t, reservedLookup)))
            .ToList();
    }

    private static int? ResolveAvailableQuantity(
        EventTicketType ticketType,
        Dictionary<Guid, int> reservedLookup)
    {
        if (!ticketType.Capacity.HasValue)
            return null;

        reservedLookup.TryGetValue(ticketType.Id, out var reserved);
        var available = ticketType.Capacity.Value - reserved;
        return available < 0 ? 0 : available;
    }

    private sealed record PriceInfo(decimal? PriceFrom, string? Currency);
}
