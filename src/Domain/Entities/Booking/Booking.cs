using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Domain.Entities.Sports;
using Bookazone.Domain.Entities.Tenant;
using Bookazone.Domain.Enums;

namespace Bookazone.Domain.Entities.Booking;

public class Booking : UserBaseEntity
{
    public Guid FkTenantId { get; set; }
    public Tenants FkTenant { get; set; } = null!;
    public Guid FkSportResourceId { get; set; }
    public SportResource FkSportResource { get; set; } = null!;
    public Guid FkCustomerId { get; set; }
    public Users FkCustomer { get; set; } = null!;
    public BookingStatus Status { get; set; } = BookingStatus.PendingApproval;
    public BookingMode BookingMode { get; set; } = BookingMode.Instant;
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public int? ExpectedAttendees { get; set; }
    public decimal TotalPrice { get; set; }
    [MaxLength(3)] public string Currency { get; set; } = "USD";
    [MaxLength(200000)] public string? Notes { get; set; }
}