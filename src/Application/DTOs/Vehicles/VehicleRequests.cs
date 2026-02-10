using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Vehicles;

public class VehicleSearchRequest
{
    public Guid? TenantId { get; set; }
    public VehicleServiceType? ServiceType { get; set; }
    public VehicleType? VehicleType { get; set; }
    public string? City { get; set; }
    public string? Search { get; set; }
    public int? MinSeats { get; set; }
    public VehicleTransmissionType? Transmission { get; set; }
    public VehicleFuelType? FuelType { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
}

public class VehicleCreateRequest
{
    public Guid TenantId { get; set; }
    public VehicleServiceType ServiceType { get; set; } = VehicleServiceType.Rental;
    public VehicleType Type { get; set; } = VehicleType.Car;
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
    public VehicleDriverOption DriverOption { get; set; } = VehicleDriverOption.Optional;
    public string? FuelPolicy { get; set; }
    public int? MinBookingDuration { get; set; }
    public int? MaxBookingDuration { get; set; }
    public VehicleDurationUnit? BookingDurationUnit { get; set; }
    public decimal? MaxLoadTons { get; set; }
    public decimal? CargoVolumeCubicMeters { get; set; }
    public BookingMode BookingMode { get; set; } = BookingMode.Instant;
    public decimal? PriceFrom { get; set; }
    public string? Currency { get; set; }
    public List<VehiclePricingRuleUpsertRequest>? PricingRules { get; set; }
    public List<VehicleSpecUpsertRequest>? Specs { get; set; }
    public List<VehiclePolicyUpsertRequest>? Policies { get; set; }
    public List<VehicleMediaUpsertRequest>? Media { get; set; }
}

public class VehicleUpdateRequest
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public VehicleServiceType ServiceType { get; set; } = VehicleServiceType.Rental;
    public VehicleType Type { get; set; } = VehicleType.Car;
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
    public VehicleDriverOption DriverOption { get; set; } = VehicleDriverOption.Optional;
    public string? FuelPolicy { get; set; }
    public int? MinBookingDuration { get; set; }
    public int? MaxBookingDuration { get; set; }
    public VehicleDurationUnit? BookingDurationUnit { get; set; }
    public decimal? MaxLoadTons { get; set; }
    public decimal? CargoVolumeCubicMeters { get; set; }
    public BookingMode BookingMode { get; set; } = BookingMode.Instant;
    public decimal? PriceFrom { get; set; }
    public string? Currency { get; set; }
}

public class VehiclePublishRequest
{
    public Guid VehicleId { get; set; }
}

public class VehicleStatusUpdateRequest
{
    public Guid VehicleId { get; set; }
    public VehicleStatus Status { get; set; }
}

public class VehiclePricingRuleUpsertRequest
{
    public Guid? Id { get; set; }
    public VehiclePricingUnit Unit { get; set; } = VehiclePricingUnit.Day;
    public decimal PriceAmount { get; set; }
    public string? Currency { get; set; }
    public decimal? MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
    public bool IsPrimary { get; set; }
}

public class VehicleSpecUpsertRequest
{
    public Guid? Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class VehiclePolicyUpsertRequest
{
    public Guid? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public int SortOrder { get; set; }
}

public class VehicleMediaUpsertRequest
{
    public Guid? Id { get; set; }
    public Guid StoredFileId { get; set; }
    public bool IsCover { get; set; }
    public int SortOrder { get; set; }
}
