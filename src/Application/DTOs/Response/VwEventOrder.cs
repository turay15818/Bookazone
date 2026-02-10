using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Response;

public class VwEventOrderItem
{
    public Guid TicketTypeId { get; set; }
    public string TicketName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal Subtotal { get; set; }
}

public class VwEventOrderEvent
{
    public string Title { get; set; } = string.Empty;
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public string? City { get; set; }
    public string? VenueName { get; set; }
    public string? CoverUrl { get; set; }
}

public class VwEventOrder
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid TenantId { get; set; }
    public Guid CustomerId { get; set; }
    public EventOrderStatus Status { get; set; }
    public EventPaymentStatus PaymentStatus { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public int TotalQuantity { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public DateTime? CreatedAtUtc { get; set; }
    public string? Notes { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public List<VwEventOrderItem> Items { get; set; } = new();
    public VwEventOrderEvent? Event { get; set; }
}

public class VwEventOrderListItem
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public EventOrderStatus Status { get; set; }
    public EventPaymentStatus PaymentStatus { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public int TotalQuantity { get; set; }
    public DateTime? CreatedAtUtc { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
}
