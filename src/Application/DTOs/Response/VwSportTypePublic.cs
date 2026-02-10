using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Response;

public class VwSportTypePublic
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string? TenantName { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public BookingMode BookingMode { get; set; }
    public int? MinDurationMinutes { get; set; }
    public int? MaxDurationMinutes { get; set; }
    public int? BufferMinutes { get; set; }
}