using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Tenant;

namespace Bookazone.Domain.Entities.Events;

public class Event : UserBaseEntity
{
    public Guid FkTenantId { get; set; }
    public Tenants FkTenant { get; set; } = null!;
    public Guid FkEventCategoryId { get; set; }
    public EventCategory FkEventCategory { get; set; } = null!;
    [MaxLength(200)] public string Title { get; set; } = null!;
    [MaxLength(300)] public string? Subtitle { get; set; }
    [MaxLength(5000)] public string? About { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    [MaxLength(100)] public string? TimeZoneId { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Draft;
    [MaxLength(100)] public string? City { get; set; }
    public decimal? PriceFrom { get; set; }
    public Guid? FkVenueId { get; set; }
    public EventVenue? FkVenue { get; set; }
    public Guid? FkCoverMediaId { get; set; }
    public EventMedia? FkCoverMedia { get; set; }
}
