using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Booking;
using Bookazone.Domain.Entities.Tenant;
using Bookazone.Domain.Enums;

namespace Bookazone.Domain.Entities.Sports;

public class TenantSportType : UserBaseEntity
{
    public Guid FkTenantId { get; set; }
    public Tenants FkTenant { get; set; } = null!;
    public Guid FkTenantBookingCategoryId { get; set; }
    public TenantBookingCategory FkTenantBookingCategory { get; set; } = null!;
    [MaxLength(150)] public string Name { get; set; } = null!;
    [MaxLength(1000)] public string? Description { get; set; }
    public BookingMode BookingMode { get; set; } = BookingMode.Instant;
    public int? MinDurationMinutes { get; set; }
    public int? MaxDurationMinutes { get; set; }
    public int? BufferMinutes { get; set; }
}