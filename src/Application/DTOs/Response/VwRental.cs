using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Response;

public class VwRentalPricingRule
{
    public Guid Id { get; set; }
    public Guid RentalId { get; set; }
    public RentalPricingUnit Unit { get; set; }
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
    public bool IsPrimary { get; set; }
}

public class VwRentalSpec
{
    public Guid Id { get; set; }
    public Guid RentalId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class VwRentalPolicy
{
    public Guid Id { get; set; }
    public Guid RentalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public int SortOrder { get; set; }
}

public class VwRentalMedia
{
    public Guid Id { get; set; }
    public Guid RentalId { get; set; }
    public Guid StoredFileId { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool IsCover { get; set; }
    public int SortOrder { get; set; }
}

public class VwRentalCard
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public RentalType Type { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public int? Capacity { get; set; }
    public int? Bedrooms { get; set; }
    public int UnitsAvailable { get; set; }
    public BookingMode BookingMode { get; set; }
    public decimal? PriceFrom { get; set; }
    public string? Currency { get; set; }
    public RentalPricingUnit? PriceUnit { get; set; }
    public string? PriceLabel { get; set; }
    public string? CoverUrl { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string? TenantLogo { get; set; }
    public bool TenantVerified { get; set; }
}

public class VwRentalListItem : VwRentalCard
{
    public RentalStatus Status { get; set; }
}

public class VwRentalDetail
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
    public RentalType Type { get; set; }
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
    public int UnitsAvailable { get; set; }
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
    public BookingMode BookingMode { get; set; }
    public RentalStatus Status { get; set; }
    public decimal? PriceFrom { get; set; }
    public string? Currency { get; set; }
    public RentalPricingUnit? PriceUnit { get; set; }
    public string? PriceLabel { get; set; }
    public string? CoverUrl { get; set; }
    public List<VwRentalPricingRule> Pricing { get; set; } = new();
    public List<VwRentalSpec> Specs { get; set; } = new();
    public List<VwRentalPolicy> Policies { get; set; } = new();
    public List<VwRentalMedia> Media { get; set; } = new();
}
