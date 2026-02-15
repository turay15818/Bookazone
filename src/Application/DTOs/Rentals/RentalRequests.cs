using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Rentals;

public class RentalSearchRequest
{
    public Guid? TenantId { get; set; }
    public RentalType? Type { get; set; }
    public string? City { get; set; }
    public string? Search { get; set; }
    public int? MinCapacity { get; set; }
    public int? MinBedrooms { get; set; }
    public int? MinBathrooms { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
}

public class RentalCreateRequest
{
    public Guid TenantId { get; set; }
    public RentalType Type { get; set; } = RentalType.Hall;
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? Capacity { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public decimal? FloorAreaSquareMeters { get; set; }
    public int UnitsAvailable { get; set; } = 1;
    public bool? HasToilet { get; set; }
    public bool? HasAirConditioning { get; set; }
    public bool? HasFan { get; set; }
    public bool? HasSoundSystem { get; set; }
    public bool? HasWifi { get; set; }
    public bool? HasParking { get; set; }
    public int? MinBookingDuration { get; set; }
    public int? MaxBookingDuration { get; set; }
    public RentalPricingUnit? BookingDurationUnit { get; set; }
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public BookingMode BookingMode { get; set; } = BookingMode.Instant;
    public decimal? PriceFrom { get; set; }
    public string? Currency { get; set; }
    public List<RentalPricingRuleUpsertRequest>? PricingRules { get; set; }
    public List<RentalSpecUpsertRequest>? Specs { get; set; }
    public List<RentalPolicyUpsertRequest>? Policies { get; set; }
    public List<RentalMediaUpsertRequest>? Media { get; set; }
}

public class RentalUpdateRequest
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public RentalType Type { get; set; } = RentalType.Hall;
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? Capacity { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public decimal? FloorAreaSquareMeters { get; set; }
    public int UnitsAvailable { get; set; } = 1;
    public bool? HasToilet { get; set; }
    public bool? HasAirConditioning { get; set; }
    public bool? HasFan { get; set; }
    public bool? HasSoundSystem { get; set; }
    public bool? HasWifi { get; set; }
    public bool? HasParking { get; set; }
    public int? MinBookingDuration { get; set; }
    public int? MaxBookingDuration { get; set; }
    public RentalPricingUnit? BookingDurationUnit { get; set; }
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public BookingMode BookingMode { get; set; } = BookingMode.Instant;
    public decimal? PriceFrom { get; set; }
    public string? Currency { get; set; }
}

public class RentalPublishRequest
{
    public Guid RentalId { get; set; }
}

public class RentalStatusUpdateRequest
{
    public Guid RentalId { get; set; }
    public RentalStatus Status { get; set; }
}

public class RentalPricingRuleUpsertRequest
{
    public Guid? Id { get; set; }
    public RentalPricingUnit Unit { get; set; } = RentalPricingUnit.Day;
    public decimal PriceAmount { get; set; }
    public string? Currency { get; set; }
    public decimal? MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
    public bool IsPrimary { get; set; }
}

public class RentalSpecUpsertRequest
{
    public Guid? Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class RentalPolicyUpsertRequest
{
    public Guid? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public int SortOrder { get; set; }
}

public class RentalMediaUpsertRequest
{
    public Guid? Id { get; set; }
    public Guid StoredFileId { get; set; }
    public bool IsCover { get; set; }
    public int SortOrder { get; set; }
}
