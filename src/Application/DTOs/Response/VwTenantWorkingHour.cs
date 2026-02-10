namespace Bookazone.Application.DTOs.Response;

public class VwTenantWorkingHour
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }
    public bool IsClosed { get; set; }
    public int SlotDurationMinutes { get; set; }
}
