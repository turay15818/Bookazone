namespace Bookazone.Application.DTOs.Events;

public class EventSearchRequest
{
    public Guid? TenantId { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryCode { get; set; }
    public string? City { get; set; }
    public string? Search { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
}

public class EventCategoryCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Active { get; set; } = true;
}

public class EventCategoryUpdateRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Active { get; set; } = true;
}

public class EventCreateRequest
{
    public Guid TenantId { get; set; }
    public Guid EventCategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? About { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public string? TimeZoneId { get; set; }
    public string? City { get; set; }
    public decimal? PriceFrom { get; set; }
    public EventVenueUpsertRequest? Venue { get; set; }
    public List<EventTicketTypeUpsertRequest>? TicketTypes { get; set; }
    public List<EventScheduleItemUpsertRequest>? Schedule { get; set; }
    public List<EventPolicyUpsertRequest>? Policies { get; set; }
    public List<EventMediaUpsertRequest>? Media { get; set; }
}

public class EventUpdateRequest
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EventCategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? About { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public string? TimeZoneId { get; set; }
    public string? City { get; set; }
    public decimal? PriceFrom { get; set; }
}

public class EventPublishRequest
{
    public Guid EventId { get; set; }
}

public class EventCancelRequest
{
    public Guid EventId { get; set; }
}

public class EventVenueUpsertRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class EventTicketTypeUpsertRequest
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PriceAmount { get; set; }
    public string? Currency { get; set; }
    public int? Capacity { get; set; }
    public int? MinPerOrder { get; set; }
    public int? MaxPerOrder { get; set; }
    public DateTime? SalesStartUtc { get; set; }
    public DateTime? SalesEndUtc { get; set; }
    public List<string>? Perks { get; set; }
}

public class EventScheduleItemUpsertRequest
{
    public Guid? Id { get; set; }
    public DateTime StartUtc { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public int SortOrder { get; set; }
}

public class EventPolicyUpsertRequest
{
    public Guid? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public int SortOrder { get; set; }
}

public class EventMediaUpsertRequest
{
    public Guid? Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public bool IsCover { get; set; }
    public int SortOrder { get; set; }
}

public class EventOrderCreateRequest
{
    public Guid EventId { get; set; }
    public List<EventOrderItemCreateRequest> Items { get; set; } = new();
    public EventOrderContactRequest? Contact { get; set; }
    public string? Notes { get; set; }
}

public class EventOrderItemCreateRequest
{
    public Guid TicketTypeId { get; set; }
    public int Quantity { get; set; }
}

public class EventOrderContactRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public class EventOrderConfirmRequest
{
    public Guid OrderId { get; set; }
}

public class EventOrderCancelRequest
{
    public Guid OrderId { get; set; }
    public string? Reason { get; set; }
}

public class EventCategorySubscriptionUpsertRequest
{
    public Guid EventCategoryId { get; set; }
    public bool Subscribe { get; set; } = true;
    public bool ReceivePush { get; set; } = true;
    public bool ReceiveEmail { get; set; } = true;
}
