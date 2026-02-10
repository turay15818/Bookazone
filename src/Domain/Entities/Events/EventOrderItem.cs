using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Events;

public class EventOrderItem : UserBaseEntity
{
    public Guid FkEventOrderId { get; set; }
    public EventOrder FkEventOrder { get; set; } = null!;
    public Guid FkEventTicketTypeId { get; set; }
    public EventTicketType FkEventTicketType { get; set; } = null!;
    [MaxLength(100)] public string TicketName { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    [MaxLength(3)] public string Currency { get; set; } = "USD";
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
}
