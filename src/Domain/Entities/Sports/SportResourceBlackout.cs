using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Sports;

public class SportResourceBlackout : UserBaseEntity
{
    public Guid FkSportResourceId { get; set; }
    public SportResource FkSportResource { get; set; } = null!;
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    [MaxLength(250)] public string? Reason { get; set; }
}
