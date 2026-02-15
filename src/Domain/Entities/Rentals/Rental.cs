using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Tenant;
using Bookazone.Domain.Enums;

namespace Bookazone.Domain.Entities.Rentals;

public class Rental : UserBaseEntity
{
    public Guid FkTenantId { get; set; }
    public Tenants FkTenant { get; set; } = null!;
    public RentalType Type { get; set; } = RentalType.Hall;
    [MaxLength(150)] public string Title { get; set; } = null!;
    [MaxLength(300)] public string? Subtitle { get; set; }
    [MaxLength(5000)] public string? Description { get; set; }
    [MaxLength(100)] public string? City { get; set; }
    [MaxLength(300)] public string? Address { get; set; }
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
    public RentalStatus Status { get; set; } = RentalStatus.Draft;
    public decimal? PriceFrom { get; set; }
    [MaxLength(3)] public string? Currency { get; set; }
    public Guid? FkCoverMediaId { get; set; }
    public RentalMedia? FkCoverMedia { get; set; }
}
