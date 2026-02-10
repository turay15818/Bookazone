using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Response;

public class VwVehicleOrderListItem
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string VehicleTitle { get; set; } = string.Empty;
    public VehicleType VehicleType { get; set; }
    public VehicleServiceType ServiceType { get; set; }
    public string? VehicleCoverUrl { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public VehicleOrderStatus Status { get; set; }
    public VehiclePricingUnit PricingUnit { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime? StartUtc { get; set; }
    public DateTime? EndUtc { get; set; }
    public DateTime? DateCreated { get; set; }
}

public class VwVehicleOrderDetail : VwVehicleOrderListItem
{
    public string? PickupAddress { get; set; }
    public string? DropoffAddress { get; set; }
    public double? PickupLatitude { get; set; }
    public double? PickupLongitude { get; set; }
    public double? DropoffLatitude { get; set; }
    public double? DropoffLongitude { get; set; }
    public string? CargoDescription { get; set; }
    public decimal? CargoWeightTons { get; set; }
    public decimal? CargoVolumeCubicMeters { get; set; }
    public bool DriverRequested { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Notes { get; set; }
}
