using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Response;

public class VwEquipmentPricingRule
{
    public Guid Id { get; set; }
    public Guid EquipmentId { get; set; }
    public EquipmentPricingUnit Unit { get; set; }
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
    public bool IsPrimary { get; set; }
}

public class VwEquipmentSpec
{
    public Guid Id { get; set; }
    public Guid EquipmentId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class VwEquipmentPolicy
{
    public Guid Id { get; set; }
    public Guid EquipmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public int SortOrder { get; set; }
}

public class VwEquipmentMedia
{
    public Guid Id { get; set; }
    public Guid EquipmentId { get; set; }
    public Guid StoredFileId { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool IsCover { get; set; }
    public int SortOrder { get; set; }
}

public class VwEquipmentCard
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Category { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public int UnitsAvailable { get; set; }
    public BookingMode BookingMode { get; set; }
    public decimal? PriceFrom { get; set; }
    public string? Currency { get; set; }
    public EquipmentPricingUnit? PriceUnit { get; set; }
    public string? PriceLabel { get; set; }
    public string? CoverUrl { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string? TenantLogo { get; set; }
    public bool TenantVerified { get; set; }
}

public class VwEquipmentListItem : VwEquipmentCard
{
    public EquipmentStatus Status { get; set; }
}

public class VwEquipmentDetail
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
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int UnitsAvailable { get; set; }
    public bool? DeliveryAvailable { get; set; }
    public bool? PickupAvailable { get; set; }
    public int? MinBookingDuration { get; set; }
    public int? MaxBookingDuration { get; set; }
    public EquipmentPricingUnit? BookingDurationUnit { get; set; }
    public BookingMode BookingMode { get; set; }
    public EquipmentStatus Status { get; set; }
    public decimal? PriceFrom { get; set; }
    public string? Currency { get; set; }
    public EquipmentPricingUnit? PriceUnit { get; set; }
    public string? PriceLabel { get; set; }
    public string? CoverUrl { get; set; }
    public List<VwEquipmentPricingRule> Pricing { get; set; } = new();
    public List<VwEquipmentSpec> Specs { get; set; } = new();
    public List<VwEquipmentPolicy> Policies { get; set; } = new();
    public List<VwEquipmentMedia> Media { get; set; } = new();
}
