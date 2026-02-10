using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Tenant;
using Bookazone.Domain.Enums;

namespace Bookazone.Domain.Entities.Vehicles;

public class Vehicle : UserBaseEntity
{
    public Guid FkTenantId { get; set; }
    public Tenants FkTenant { get; set; } = null!;
    public VehicleServiceType ServiceType { get; set; } = VehicleServiceType.Rental;
    public VehicleType Type { get; set; } = VehicleType.Car;
    [MaxLength(150)] public string Title { get; set; } = null!;
    [MaxLength(300)] public string? Subtitle { get; set; }
    [MaxLength(5000)] public string? Description { get; set; }
    [MaxLength(100)] public string? City { get; set; }
    [MaxLength(300)] public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    [MaxLength(100)] public string? Make { get; set; }
    [MaxLength(100)] public string? Model { get; set; }
    public int? Year { get; set; }
    public int? Seats { get; set; }
    public VehicleTransmissionType? Transmission { get; set; }
    public VehicleFuelType? FuelType { get; set; }
    public bool? HasAirConditioning { get; set; }
    public bool? HasInsurance { get; set; }
    public bool? DeliveryAvailable { get; set; }
    public VehicleDriverOption DriverOption { get; set; } = VehicleDriverOption.Optional;
    [MaxLength(200)] public string? FuelPolicy { get; set; }
    public int? MinBookingDuration { get; set; }
    public int? MaxBookingDuration { get; set; }
    public VehicleDurationUnit? BookingDurationUnit { get; set; }
    public decimal? MaxLoadTons { get; set; }
    public decimal? CargoVolumeCubicMeters { get; set; }
    public BookingMode BookingMode { get; set; } = BookingMode.Instant;
    public VehicleStatus Status { get; set; } = VehicleStatus.Draft;
    public decimal? PriceFrom { get; set; }
    [MaxLength(3)] public string? Currency { get; set; }
    public Guid? FkCoverMediaId { get; set; }
    public VehicleMedia? FkCoverMedia { get; set; }
}
