using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Response;

public class VwBooking
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ResourceId { get; set; }
    public Guid CustomerId { get; set; }
    public BookingStatus Status { get; set; }
    public BookingMode BookingMode { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public int? ExpectedAttendees { get; set; }
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Notes { get; set; }
}
