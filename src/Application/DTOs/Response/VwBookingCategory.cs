using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Response;

public class VwBookingCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public BookingMode DefaultBookingMode { get; set; }
    public bool Active { get; set; }
}
