using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Equipment;

public class EquipmentOrderCreateRequest
{
    public Guid EquipmentId { get; set; }
    public EquipmentPricingUnit PricingUnit { get; set; }
    public decimal? Quantity { get; set; }
    public int? UnitsRequested { get; set; }
    public DateTime? StartUtc { get; set; }
    public DateTime? EndUtc { get; set; }
    public string? DeliveryAddress { get; set; }
    public double? DeliveryLatitude { get; set; }
    public double? DeliveryLongitude { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Notes { get; set; }
}

public class EquipmentOrderStatusUpdateRequest
{
    public Guid OrderId { get; set; }
    public EquipmentOrderStatus Status { get; set; }
}

public class EquipmentOrderCancelRequest
{
    public Guid OrderId { get; set; }
}
