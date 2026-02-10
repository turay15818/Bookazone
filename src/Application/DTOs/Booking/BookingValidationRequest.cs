namespace Bookazone.Application.DTOs.Booking;

public class BookingValidationRequest
{
    public Guid? BookingId { get; set; }
    public Guid TenantId { get; set; }
    public Guid ResourceId { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public int? ExpectedAttendees { get; set; }
}
