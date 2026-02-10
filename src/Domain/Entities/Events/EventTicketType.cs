using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Events;

public class EventTicketType : UserBaseEntity
{
    public Guid FkEventId { get; set; }
    public Event FkEvent { get; set; } = null!;
    [MaxLength(100)] public string Name { get; set; } = null!;
    public decimal PriceAmount { get; set; }
    [MaxLength(3)] public string Currency { get; set; } = "USD";
    public int? Capacity { get; set; }
    public int? MinPerOrder { get; set; }
    public int? MaxPerOrder { get; set; }
    public DateTime? SalesStartUtc { get; set; }
    public DateTime? SalesEndUtc { get; set; }
    [MaxLength(2000)] public string? Perks { get; set; }
}
