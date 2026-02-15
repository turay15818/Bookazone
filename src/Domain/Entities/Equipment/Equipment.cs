using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Tenant;
using Bookazone.Domain.Enums;

namespace Bookazone.Domain.Entities.Equipment;

public class Equipment : UserBaseEntity
{
    public Guid FkTenantId { get; set; }
    public Tenants FkTenant { get; set; } = null!;
    [MaxLength(150)] public string Title { get; set; } = null!;
    [MaxLength(300)] public string? Subtitle { get; set; }
    [MaxLength(5000)] public string? Description { get; set; }
    [MaxLength(100)] public string? Category { get; set; }
    [MaxLength(100)] public string? City { get; set; }
    [MaxLength(300)] public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int UnitsAvailable { get; set; } = 1;
    public bool? DeliveryAvailable { get; set; }
    public bool? PickupAvailable { get; set; }
    public int? MinBookingDuration { get; set; }
    public int? MaxBookingDuration { get; set; }
    public EquipmentPricingUnit? BookingDurationUnit { get; set; }
    public BookingMode BookingMode { get; set; } = BookingMode.Approval;
    public EquipmentStatus Status { get; set; } = EquipmentStatus.Draft;
    public decimal? PriceFrom { get; set; }
    [MaxLength(3)] public string? Currency { get; set; }
    public Guid? FkCoverMediaId { get; set; }
    public EquipmentMedia? FkCoverMedia { get; set; }
}
