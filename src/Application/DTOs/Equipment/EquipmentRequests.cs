using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Equipment;

public class EquipmentSearchRequest
{
    public Guid? TenantId { get; set; }
    public string? Category { get; set; }
    public string? City { get; set; }
    public string? Search { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinUnitsAvailable { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
}

public class EquipmentCreateRequest
{
    public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int UnitsAvailable { get; set; } = 1;
    public bool? DeliveryAvailable { get; set; }
    public bool? PickupAvailable { get; set; }
    public int? MinBookingDuration { get; set; }
    public int? MaxBookingDuration { get; set; }
    public EquipmentPricingUnit? BookingDurationUnit { get; set; }
    public BookingMode BookingMode { get; set; } = BookingMode.Approval;
    public decimal? PriceFrom { get; set; }
    public string? Currency { get; set; }
    public List<EquipmentPricingRuleUpsertRequest>? PricingRules { get; set; }
    public List<EquipmentSpecUpsertRequest>? Specs { get; set; }
    public List<EquipmentPolicyUpsertRequest>? Policies { get; set; }
    public List<EquipmentMediaUpsertRequest>? Media { get; set; }
}

public class EquipmentUpdateRequest
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int UnitsAvailable { get; set; } = 1;
    public bool? DeliveryAvailable { get; set; }
    public bool? PickupAvailable { get; set; }
    public int? MinBookingDuration { get; set; }
    public int? MaxBookingDuration { get; set; }
    public EquipmentPricingUnit? BookingDurationUnit { get; set; }
    public BookingMode BookingMode { get; set; } = BookingMode.Approval;
    public decimal? PriceFrom { get; set; }
    public string? Currency { get; set; }
}

public class EquipmentPublishRequest
{
    public Guid EquipmentId { get; set; }
}

public class EquipmentStatusUpdateRequest
{
    public Guid EquipmentId { get; set; }
    public EquipmentStatus Status { get; set; }
}

public class EquipmentPricingRuleUpsertRequest
{
    public Guid? Id { get; set; }
    public EquipmentPricingUnit Unit { get; set; } = EquipmentPricingUnit.Day;
    public decimal PriceAmount { get; set; }
    public string? Currency { get; set; }
    public decimal? MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
    public bool IsPrimary { get; set; }
}

public class EquipmentSpecUpsertRequest
{
    public Guid? Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class EquipmentPolicyUpsertRequest
{
    public Guid? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public int SortOrder { get; set; }
}

public class EquipmentMediaUpsertRequest
{
    public Guid? Id { get; set; }
    public Guid StoredFileId { get; set; }
    public bool IsCover { get; set; }
    public int SortOrder { get; set; }
}
