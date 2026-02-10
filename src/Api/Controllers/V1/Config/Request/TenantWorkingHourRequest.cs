using System.ComponentModel.DataAnnotations;

namespace Bookazone.Api.Controllers.V1.Config.Request;

public class TenantWorkingHourUpsertRequest
{
    [Required] public Guid TenantId { get; set; }
    [Required] public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }
    public bool IsClosed { get; set; } = false;
    public int SlotDurationMinutes { get; set; } = 60;
}
