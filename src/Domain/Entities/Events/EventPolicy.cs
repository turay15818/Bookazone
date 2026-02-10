using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Events;

public class EventPolicy : UserBaseEntity
{
    public Guid FkEventId { get; set; }
    public Event FkEvent { get; set; } = null!;
    [MaxLength(150)] public string Title { get; set; } = null!;
    [MaxLength(2000)] public string? Body { get; set; }
    public int SortOrder { get; set; } = 0;
}
