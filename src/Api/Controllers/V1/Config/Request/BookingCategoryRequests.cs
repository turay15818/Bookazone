using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Enums;

namespace Bookazone.Api.Controllers.V1.Config.Request;

public class BookingCategoryCreateRequest
{
    [Required, MaxLength(100)] public string Name { get; set; } = null!;
    [Required, MaxLength(50)] public string Code { get; set; } = null!;
    [MaxLength(500)] public string? Description { get; set; }
    public BookingMode DefaultBookingMode { get; set; } = BookingMode.Instant;
}

public class BookingCategoryUpdateRequest
{
    [Required] public Guid Id { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = null!;
    [Required, MaxLength(50)] public string Code { get; set; } = null!;
    [MaxLength(500)] public string? Description { get; set; }
    public BookingMode DefaultBookingMode { get; set; } = BookingMode.Instant;
}

public class TenantBookingCategoryUpsertRequest
{
    [Required] public Guid TenantId { get; set; }
    [Required] public Guid BookingCategoryId { get; set; }
    public bool IsEnabled { get; set; } = true;
    public BookingMode? BookingModeOverride { get; set; }
}
