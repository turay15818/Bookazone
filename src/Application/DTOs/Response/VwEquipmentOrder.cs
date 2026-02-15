using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Response;

public class VwEquipmentOrderListItem
{
    public Guid Id { get; set; }
    public Guid EquipmentId { get; set; }
    public string EquipmentTitle { get; set; } = string.Empty;
    public string? EquipmentCategory { get; set; }
    public string? EquipmentCoverUrl { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public EquipmentOrderStatus Status { get; set; }
    public EquipmentPricingUnit PricingUnit { get; set; }
    public decimal Quantity { get; set; }
    public int UnitsRequested { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime? StartUtc { get; set; }
    public DateTime? EndUtc { get; set; }
    public DateTime? DateCreated { get; set; }
}

public class VwEquipmentOrderDetail : VwEquipmentOrderListItem
{
    public string? DeliveryAddress { get; set; }
    public double? DeliveryLatitude { get; set; }
    public double? DeliveryLongitude { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Notes { get; set; }
}
