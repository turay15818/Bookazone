namespace Bookazone.Application.DTOs.Booking;

public class BookingDecisionRequest
{
    public Guid BookingId { get; set; }
    public string? Reason { get; set; }
}
