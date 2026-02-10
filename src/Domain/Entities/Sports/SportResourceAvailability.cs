using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Sports;

public class SportResourceAvailability : UserBaseEntity
{
    public Guid FkSportResourceId { get; set; }
    public SportResource FkSportResource { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }
    public bool IsClosed { get; set; } = false;
}
