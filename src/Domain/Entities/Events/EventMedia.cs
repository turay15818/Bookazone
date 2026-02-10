using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Events;

public class EventMedia : UserBaseEntity
{
    public Guid FkEventId { get; set; }
    public Event FkEvent { get; set; } = null!;
    [MaxLength(500)] public string Url { get; set; } = null!;
    [MaxLength(200)] public string? Caption { get; set; }
    public bool IsCover { get; set; } = false;
    public int SortOrder { get; set; } = 0;
}
