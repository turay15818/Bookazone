using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Response;

public class VwRentalOrderListItem
{
    public Guid Id { get; set; }
    public Guid RentalId { get; set; }
    public string RentalTitle { get; set; } = string.Empty;
    public RentalType RentalType { get; set; }
    public string? RentalCoverUrl { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public RentalOrderStatus Status { get; set; }
    public RentalPricingUnit PricingUnit { get; set; }
    public decimal Quantity { get; set; }
    public int UnitsRequested { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime? StartUtc { get; set; }
    public DateTime? EndUtc { get; set; }
    public DateTime? DateCreated { get; set; }
}

public class VwRentalOrderDetail : VwRentalOrderListItem
{
    public int? GuestCount { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Notes { get; set; }
}
