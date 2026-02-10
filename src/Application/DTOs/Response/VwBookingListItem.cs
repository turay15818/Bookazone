using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Response;

public class VwBookingListItem
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public Guid ResourceId { get; set; }
    public string ResourceName { get; set; } = string.Empty;
    public Guid? SportTypeId { get; set; }
    public string? SportTypeName { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public BookingStatus Status { get; set; }
    public BookingMode BookingMode { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public int? ExpectedAttendees { get; set; }
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Notes { get; set; }
}
