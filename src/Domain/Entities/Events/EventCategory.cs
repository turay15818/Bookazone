using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Events;

public class EventCategory : UserBaseEntity
{
    [MaxLength(150)] public string Name { get; set; } = null!;
    [MaxLength(50)] public string Code { get; set; } = null!;
    [MaxLength(500)] public string? Description { get; set; }
}
