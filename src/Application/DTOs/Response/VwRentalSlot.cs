namespace Bookazone.Application.DTOs.Response;

public class VwRentalSlot
{
    public DateTime StartLocal { get; set; }
    public DateTime EndLocal { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public bool IsAvailable { get; set; }
    public string? UnavailableReason { get; set; }
    public int ReservedUnits { get; set; }
}

public class VwRentalSlotSummary
{
    public Guid RentalId { get; set; }
    public DateTime DateLocal { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }
    public int SlotDurationMinutes { get; set; }
    public bool IsClosed { get; set; }
    public string? StatusMessage { get; set; }
    public int UnitsAvailable { get; set; }
    public List<VwRentalSlot> Slots { get; set; } = new();
}
