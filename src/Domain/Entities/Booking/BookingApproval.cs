using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;
using Bookazone.Domain.Enums;

namespace Bookazone.Domain.Entities.Booking;

public class BookingApproval : UserBaseEntity
{
    public Guid FkBookingId { get; set; }
    public Booking FkBooking { get; set; } = null!;
    public BookingApprovalDecision Decision { get; set; } = BookingApprovalDecision.Pending;
    public DateTime? DecisionAtUtc { get; set; }
    [MaxLength(1000)] public string? Reason { get; set; }
}
