using System.Text.Json;
using Bookazone.Application.DTOs.Response;
using Bookazone.Domain.Entities.Events;
using Bookazone.Domain.Entities.Tenant;

namespace Bookazone.Application.DTOs.Converter;

public static class EventConverter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static VwEventCategory ToDto(EventCategory category)
    {
        return new VwEventCategory
        {
            Id = category.Id,
            Name = category.Name,
            Code = category.Code,
            Description = category.Description,
            Active = category.Active
        };
    }

    public static VwEventCategorySubscription ToDto(EventCategorySubscription subscription)
    {
        var category = subscription.FkEventCategory;
        return new VwEventCategorySubscription
        {
            Id = subscription.Id,
            EventCategoryId = subscription.FkEventCategoryId,
            EventCategoryName = category?.Name ?? string.Empty,
            EventCategoryCode = category?.Code ?? string.Empty,
            EventCategoryDescription = category?.Description,
            ReceivePush = subscription.ReceivePush,
            ReceiveEmail = subscription.ReceiveEmail,
            Active = subscription.Active
        };
    }

    public static VwEventVenue ToDto(EventVenue venue)
    {
        return new VwEventVenue
        {
            Id = venue.Id,
            Name = venue.Name,
            Address = venue.Address,
            City = venue.City,
            State = venue.State,
            Country = venue.Country,
            Latitude = venue.Latitude,
            Longitude = venue.Longitude
        };
    }

    public static VwEventTicketType ToDto(EventTicketType ticketType)
    {
        return new VwEventTicketType
        {
            Id = ticketType.Id,
            EventId = ticketType.FkEventId,
            Name = ticketType.Name,
            PriceAmount = ticketType.PriceAmount,
            Currency = ticketType.Currency,
            Capacity = ticketType.Capacity,
            MinPerOrder = ticketType.MinPerOrder,
            MaxPerOrder = ticketType.MaxPerOrder,
            SalesStartUtc = ticketType.SalesStartUtc,
            SalesEndUtc = ticketType.SalesEndUtc,
            Perks = ParsePerks(ticketType.Perks)
        };
    }

    public static VwEventTicketType ToDto(EventTicketType ticketType, int? availableQuantity)
    {
        var dto = ToDto(ticketType);
        dto.AvailableQuantity = availableQuantity;
        return dto;
    }

    public static VwEventScheduleItem ToDto(EventScheduleItem item)
    {
        return new VwEventScheduleItem
        {
            Id = item.Id,
            EventId = item.FkEventId,
            StartUtc = item.StartUtc,
            Title = item.Title,
            Subtitle = item.Subtitle,
            SortOrder = item.SortOrder
        };
    }

    public static VwEventPolicy ToDto(EventPolicy policy)
    {
        return new VwEventPolicy
        {
            Id = policy.Id,
            EventId = policy.FkEventId,
            Title = policy.Title,
            Body = policy.Body,
            SortOrder = policy.SortOrder
        };
    }

    public static VwEventMedia ToDto(EventMedia media)
    {
        return new VwEventMedia
        {
            Id = media.Id,
            EventId = media.FkEventId,
            Url = media.Url,
            Caption = media.Caption,
            IsCover = media.IsCover,
            SortOrder = media.SortOrder
        };
    }

    public static VwEventCard ToCardDto(
        Event evt,
        EventCategory category,
        Tenants tenant,
        EventVenue? venue,
        EventMedia? cover,
        decimal? priceFrom,
        string? currency)
    {
        return new VwEventCard
        {
            Id = evt.Id,
            Title = evt.Title,
            Subtitle = evt.Subtitle,
            StartUtc = evt.StartUtc,
            EndUtc = evt.EndUtc,
            TimeZoneId = evt.TimeZoneId,
            City = evt.City,
            VenueName = venue?.Name,
            PriceFrom = priceFrom,
            Currency = currency,
            CoverUrl = cover?.Url,
            TenantId = tenant.Id,
            TenantName = tenant.Name,
            TenantLogo = tenant.Logo,
            TenantVerified = tenant.IsVerified,
            EventCategoryId = category.Id,
            EventCategoryName = category.Name,
            EventCategoryCode = category.Code
        };
    }

    public static VwEventListItem ToListItemDto(
        Event evt,
        EventCategory category,
        Tenants tenant,
        EventVenue? venue,
        EventMedia? cover,
        decimal? priceFrom,
        string? currency)
    {
        var card = ToCardDto(evt, category, tenant, venue, cover, priceFrom, currency);
        return new VwEventListItem
        {
            Id = card.Id,
            Title = card.Title,
            Subtitle = card.Subtitle,
            StartUtc = card.StartUtc,
            EndUtc = card.EndUtc,
            TimeZoneId = card.TimeZoneId,
            City = card.City,
            VenueName = card.VenueName,
            PriceFrom = card.PriceFrom,
            Currency = card.Currency,
            CoverUrl = card.CoverUrl,
            TenantId = card.TenantId,
            TenantName = card.TenantName,
            TenantLogo = card.TenantLogo,
            TenantVerified = card.TenantVerified,
            EventCategoryId = card.EventCategoryId,
            EventCategoryName = card.EventCategoryName,
            EventCategoryCode = card.EventCategoryCode,
            Status = evt.Status
        };
    }

    public static VwEventDetail ToDetailDto(
        Event evt,
        EventCategory category,
        Tenants tenant,
        EventVenue? venue,
        string? coverUrl,
        IEnumerable<VwEventTicketType> ticketTypes,
        IEnumerable<EventScheduleItem> schedule,
        IEnumerable<EventPolicy> policies,
        IEnumerable<EventMedia> media,
        decimal? priceFrom,
        string? currency)
    {
        return new VwEventDetail
        {
            Id = evt.Id,
            TenantId = tenant.Id,
            TenantName = tenant.Name,
            TenantAddress = tenant.Address,
            TenantCity = tenant.City,
            TenantState = tenant.State,
            TenantCountry = tenant.Country,
            TenantLogo = tenant.Logo,
            TenantVerified = tenant.IsVerified,
            EventCategoryId = category.Id,
            EventCategoryName = category.Name,
            EventCategoryCode = category.Code,
            Title = evt.Title,
            Subtitle = evt.Subtitle,
            About = evt.About,
            StartUtc = evt.StartUtc,
            EndUtc = evt.EndUtc,
            TimeZoneId = evt.TimeZoneId,
            Status = evt.Status,
            City = evt.City,
            PriceFrom = priceFrom,
            Currency = currency,
            CoverUrl = coverUrl,
            Venue = venue == null ? null : ToDto(venue),
            TicketTypes = ticketTypes.ToList(),
            Schedule = schedule.Select(ToDto).ToList(),
            Policies = policies.Select(ToDto).ToList(),
            Media = media.Select(ToDto).ToList()
        };
    }

    public static string? SerializePerks(IEnumerable<string>? perks)
    {
        if (perks == null)
            return null;

        var cleaned = perks
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim())
            .ToList();

        return cleaned.Count == 0
            ? null
            : JsonSerializer.Serialize(cleaned, JsonOptions);
    }

    private static List<string> ParsePerks(string? perks)
    {
        if (string.IsNullOrWhiteSpace(perks))
            return new List<string>();

        try
        {
            var list = JsonSerializer.Deserialize<List<string>>(perks, JsonOptions);
            if (list != null)
            {
                return list
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => p.Trim())
                    .ToList();
            }
        }
        catch (JsonException)
        {
        }

        var parts = perks
            .Split(new[] { '\n', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .ToList();

        return parts.Count > 0 ? parts : new List<string> { perks.Trim() };
    }
}
