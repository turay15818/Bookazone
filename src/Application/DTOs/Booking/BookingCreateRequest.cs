using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Booking;

public class BookingCreateRequest
{
    public Guid TenantId { get; set; }
    public Guid ResourceId { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public int? ExpectedAttendees { get; set; }
    public decimal? TotalPrice { get; set; }
    public string? Currency { get; set; }
    public string? Notes { get; set; }
    public BookingMode BookingMode { get; set; } = BookingMode.Instant;
}
