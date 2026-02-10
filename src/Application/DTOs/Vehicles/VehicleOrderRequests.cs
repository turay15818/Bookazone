using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Vehicles;

public class VehicleOrderCreateRequest
{
    public Guid VehicleId { get; set; }
    public VehiclePricingUnit PricingUnit { get; set; }
    public decimal? Quantity { get; set; }
    public DateTime? StartUtc { get; set; }
    public DateTime? EndUtc { get; set; }
    public string? PickupAddress { get; set; }
    public string? DropoffAddress { get; set; }
    public double? PickupLatitude { get; set; }
    public double? PickupLongitude { get; set; }
    public double? DropoffLatitude { get; set; }
    public double? DropoffLongitude { get; set; }
    public string? CargoDescription { get; set; }
    public decimal? CargoWeightTons { get; set; }
    public decimal? CargoVolumeCubicMeters { get; set; }
    public bool? DriverRequested { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Notes { get; set; }
}

public class VehicleOrderStatusUpdateRequest
{
    public Guid OrderId { get; set; }
    public VehicleOrderStatus Status { get; set; }
}

public class VehicleOrderCancelRequest
{
    public Guid OrderId { get; set; }
}

