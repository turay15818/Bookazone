using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Tenant;

public class TenantWorkingHour : UserBaseEntity
{
    public Guid FkTenantId { get; set; }
    public Tenants FkTenant { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }
    public bool IsClosed { get; set; } = false;
    public int SlotDurationMinutes { get; set; } = 60;
}
