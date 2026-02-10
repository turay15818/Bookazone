using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Domain.Entities.Tenant;
using Bookazone.Domain.Enums;

namespace Bookazone.Domain.Entities.Vehicles;

public class VehicleOrder : UserBaseEntity
{
    public Guid FkVehicleId { get; set; }
    public Vehicle FkVehicle { get; set; } = null!;
    public Guid FkTenantId { get; set; }
    public Tenants FkTenant { get; set; } = null!;
    public Guid FkCustomerId { get; set; }
    public Users FkCustomer { get; set; } = null!;
    public VehicleServiceType ServiceType { get; set; } = VehicleServiceType.Rental;
    public VehicleOrderStatus Status { get; set; } = VehicleOrderStatus.Pending;
    public VehiclePricingUnit PricingUnit { get; set; } = VehiclePricingUnit.Day;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    [MaxLength(3)] public string Currency { get; set; } = "USD";
    public DateTime? StartUtc { get; set; }
    public DateTime? EndUtc { get; set; }
    [MaxLength(300)] public string? PickupAddress { get; set; }
    [MaxLength(300)] public string? DropoffAddress { get; set; }
    public double? PickupLatitude { get; set; }
    public double? PickupLongitude { get; set; }
    public double? DropoffLatitude { get; set; }
    public double? DropoffLongitude { get; set; }
    [MaxLength(2000)] public string? CargoDescription { get; set; }
    public decimal? CargoWeightTons { get; set; }
    public decimal? CargoVolumeCubicMeters { get; set; }
    public bool DriverRequested { get; set; }
    [MaxLength(150)] public string? ContactName { get; set; }
    [MaxLength(200)] public string? ContactEmail { get; set; }
    [MaxLength(50)] public string? ContactPhone { get; set; }
    [MaxLength(2000)] public string? Notes { get; set; }
}
