using Bookazone.Domain.Entities.Events;

namespace Bookazone.Application.DTOs.Response;

public class VwEventCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Active { get; set; }
}

public class VwEventCategorySubscription
{
    public Guid Id { get; set; }
    public Guid EventCategoryId { get; set; }
    public string EventCategoryName { get; set; } = string.Empty;
    public string EventCategoryCode { get; set; } = string.Empty;
    public string? EventCategoryDescription { get; set; }
    public bool ReceivePush { get; set; }
    public bool ReceiveEmail { get; set; }
    public bool Active { get; set; }
}

public class VwEventVenue
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class VwEventTicketType
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public int? Capacity { get; set; }
    public int? AvailableQuantity { get; set; }
    public int? MinPerOrder { get; set; }
    public int? MaxPerOrder { get; set; }
    public DateTime? SalesStartUtc { get; set; }
    public DateTime? SalesEndUtc { get; set; }
    public List<string> Perks { get; set; } = new();
}

public class VwEventScheduleItem
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public DateTime StartUtc { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public int SortOrder { get; set; }
}

public class VwEventPolicy
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public int SortOrder { get; set; }
}

public class VwEventMedia
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public bool IsCover { get; set; }
    public int SortOrder { get; set; }
}

public class VwEventCard
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public string? TimeZoneId { get; set; }
    public string? City { get; set; }
    public string? VenueName { get; set; }
    public decimal? PriceFrom { get; set; }
    public string? Currency { get; set; }
    public string? CoverUrl { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string? TenantLogo { get; set; }
    public bool TenantVerified { get; set; }
    public Guid EventCategoryId { get; set; }
    public string EventCategoryName { get; set; } = string.Empty;
    public string EventCategoryCode { get; set; } = string.Empty;
}

public class VwEventListItem : VwEventCard
{
    public EventStatus Status { get; set; }
}

public class VwEventDetail
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string? TenantAddress { get; set; }
    public string? TenantCity { get; set; }
    public string? TenantState { get; set; }
    public string? TenantCountry { get; set; }
    public string? TenantLogo { get; set; }
    public bool TenantVerified { get; set; }
    public Guid EventCategoryId { get; set; }
    public string EventCategoryName { get; set; } = string.Empty;
    public string EventCategoryCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? About { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public string? TimeZoneId { get; set; }
    public EventStatus Status { get; set; }
    public string? City { get; set; }
    public decimal? PriceFrom { get; set; }
    public string? Currency { get; set; }
    public string? CoverUrl { get; set; }
    public VwEventVenue? Venue { get; set; }
    public List<VwEventTicketType> TicketTypes { get; set; } = new();
    public List<VwEventScheduleItem> Schedule { get; set; } = new();
    public List<VwEventPolicy> Policies { get; set; } = new();
    public List<VwEventMedia> Media { get; set; } = new();
}
