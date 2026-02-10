using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Domain.Entities.Tenant;
using Bookazone.Domain.Enums;

namespace Bookazone.Domain.Entities.Events;

public class EventOrder : UserBaseEntity
{
    public Guid FkEventId { get; set; }
    public Event FkEvent { get; set; } = null!;
    public Guid FkTenantId { get; set; }
    public Tenants FkTenant { get; set; } = null!;
    public Guid FkCustomerId { get; set; }
    public Users FkCustomer { get; set; } = null!;
    public EventOrderStatus Status { get; set; } = EventOrderStatus.PendingPayment;
    public EventPaymentStatus PaymentStatus { get; set; } = EventPaymentStatus.Pending;
    public decimal TotalAmount { get; set; }
    [MaxLength(3)] public string Currency { get; set; } = "USD";
    public int TotalQuantity { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    [MaxLength(2000)] public string? Notes { get; set; }
    [MaxLength(150)] public string? ContactName { get; set; }
    [MaxLength(200)] public string? ContactEmail { get; set; }
    [MaxLength(50)] public string? ContactPhone { get; set; }
}
