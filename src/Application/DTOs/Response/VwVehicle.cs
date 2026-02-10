using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Response;

public class VwVehiclePricingRule
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public VehiclePricingUnit Unit { get; set; }
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
    public bool IsPrimary { get; set; }
}

public class VwVehicleSpec
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class VwVehiclePolicy
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public int SortOrder { get; set; }
}

public class VwVehicleMedia
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid StoredFileId { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool IsCover { get; set; }
    public int SortOrder { get; set; }
}

public class VwVehicleCard
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public VehicleServiceType ServiceType { get; set; }
    public VehicleType Type { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public int? Seats { get; set; }
    public VehicleTransmissionType? Transmission { get; set; }
    public VehicleFuelType? FuelType { get; set; }
    public BookingMode BookingMode { get; set; }
    public decimal? PriceFrom { get; set; }
    public string? Currency { get; set; }
    public VehiclePricingUnit? PriceUnit { get; set; }
    public string? PriceLabel { get; set; }
    public string? CoverUrl { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string? TenantLogo { get; set; }
    public bool TenantVerified { get; set; }
}

public class VwVehicleListItem : VwVehicleCard
{
    public VehicleStatus Status { get; set; }
}

public class VwVehicleDetail
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
    public VehicleServiceType ServiceType { get; set; }
    public VehicleType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public int? Seats { get; set; }
    public VehicleTransmissionType? Transmission { get; set; }
    public VehicleFuelType? FuelType { get; set; }
    public bool? HasAirConditioning { get; set; }
    public bool? HasInsurance { get; set; }
    public bool? DeliveryAvailable { get; set; }
    public VehicleDriverOption DriverOption { get; set; }
    public string? FuelPolicy { get; set; }
    public int? MinBookingDuration { get; set; }
    public int? MaxBookingDuration { get; set; }
    public VehicleDurationUnit? BookingDurationUnit { get; set; }
    public decimal? MaxLoadTons { get; set; }
    public decimal? CargoVolumeCubicMeters { get; set; }
    public BookingMode BookingMode { get; set; }
    public VehicleStatus Status { get; set; }
    public decimal? PriceFrom { get; set; }
    public string? Currency { get; set; }
    public VehiclePricingUnit? PriceUnit { get; set; }
    public string? PriceLabel { get; set; }
    public string? CoverUrl { get; set; }
    public List<VwVehiclePricingRule> Pricing { get; set; } = new();
    public List<VwVehicleSpec> Specs { get; set; } = new();
    public List<VwVehiclePolicy> Policies { get; set; } = new();
    public List<VwVehicleMedia> Media { get; set; } = new();
}
