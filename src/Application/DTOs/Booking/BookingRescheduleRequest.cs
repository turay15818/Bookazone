namespace Bookazone.Application.DTOs.Booking;

public class BookingRescheduleRequest
{
    public Guid BookingId { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public int? ExpectedAttendees { get; set; }
    public string? Notes { get; set; }
}
