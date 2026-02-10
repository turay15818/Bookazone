namespace Bookazone.Application.DTOs.Response;

public class VwSportResourceSlot
{
    public DateTime StartLocal { get; set; }
    public DateTime EndLocal { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public bool IsAvailable { get; set; }
    public string? UnavailableReason { get; set; }
}

public class VwSportResourceSlotSummary
{
    public Guid ResourceId { get; set; }
    public DateTime DateLocal { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }
    public int SlotDurationMinutes { get; set; }
    public bool IsClosed { get; set; }
    public string? StatusMessage { get; set; }
    public List<VwSportResourceSlot> Slots { get; set; } = new();
}
