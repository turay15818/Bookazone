namespace Bookazone.Application.DTOs.Booking;

public class BookingCancelRequest
{
    public Guid BookingId { get; set; }
    public string? Reason { get; set; }
}
