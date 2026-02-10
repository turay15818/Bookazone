using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Events;

public class EventScheduleItem : UserBaseEntity
{
    public Guid FkEventId { get; set; }
    public Event FkEvent { get; set; } = null!;
    public DateTime StartUtc { get; set; }
    [MaxLength(200)] public string Title { get; set; } = null!;
    [MaxLength(300)] public string? Subtitle { get; set; }
    public int SortOrder { get; set; } = 0;
}
