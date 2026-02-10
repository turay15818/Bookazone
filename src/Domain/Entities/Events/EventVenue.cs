using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Events;

public class EventVenue : UserBaseEntity
{
    [MaxLength(200)] public string Name { get; set; } = null!;
    [MaxLength(300)] public string? Address { get; set; }
    [MaxLength(100)] public string? City { get; set; }
    [MaxLength(100)] public string? State { get; set; }
    [MaxLength(100)] public string? Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
