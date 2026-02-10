using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Enums;

namespace Bookazone.Api.Controllers.V1.Config.Request;

public class TenantSportTypeCreateRequest
{
    [Required] public Guid TenantId { get; set; }
    [Required] public Guid TenantBookingCategoryId { get; set; }
    [Required, MaxLength(150)] public string Name { get; set; } = null!;
    [MaxLength(1000)] public string? Description { get; set; }
    public BookingMode BookingMode { get; set; } = BookingMode.Instant;
    public int? MinDurationMinutes { get; set; }
    public int? MaxDurationMinutes { get; set; }
    public int? BufferMinutes { get; set; }
}

public class TenantSportTypeUpdateRequest
{
    [Required] public Guid Id { get; set; }
    [Required] public Guid TenantId { get; set; }
    [Required] public Guid TenantBookingCategoryId { get; set; }
    [Required, MaxLength(150)] public string Name { get; set; } = null!;
    [MaxLength(1000)] public string? Description { get; set; }
    public BookingMode BookingMode { get; set; } = BookingMode.Instant;
    public int? MinDurationMinutes { get; set; }
    public int? MaxDurationMinutes { get; set; }
    public int? BufferMinutes { get; set; }
}
