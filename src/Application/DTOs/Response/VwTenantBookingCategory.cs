using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Response;

public class VwTenantBookingCategory
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid BookingCategoryId { get; set; }
    public string BookingCategoryName { get; set; } = null!;
    public string BookingCategoryCode { get; set; } = null!;
    public BookingMode DefaultBookingMode { get; set; }
    public bool IsEnabled { get; set; }
    public BookingMode? BookingModeOverride { get; set; }
    public BookingMode EffectiveBookingMode { get; set; }
}
