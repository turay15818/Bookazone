using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Tenant;
using Bookazone.Domain.Enums;

namespace Bookazone.Domain.Entities.Booking;

public class TenantBookingCategory : UserBaseEntity
{
    public Guid FkTenantId { get; set; }
    public Tenants FkTenant { get; set; } = null!;
    public Guid FkBookingCategoryId { get; set; }
    public BookingCategory FkBookingCategory { get; set; } = null!;
    public bool IsEnabled { get; set; } = true;
    public BookingMode? BookingModeOverride { get; set; }
}
