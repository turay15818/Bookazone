using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Domain.Entities.Tenant;
using Bookazone.Domain.Enums;

namespace Bookazone.Domain.Entities.Rentals;

public class RentalOrder : UserBaseEntity
{
    public Guid FkRentalId { get; set; }
    public Rental FkRental { get; set; } = null!;
    public Guid FkTenantId { get; set; }
    public Tenants FkTenant { get; set; } = null!;
    public Guid FkCustomerId { get; set; }
    public Users FkCustomer { get; set; } = null!;
    public RentalOrderStatus Status { get; set; } = RentalOrderStatus.Pending;
    public RentalPricingUnit PricingUnit { get; set; } = RentalPricingUnit.Day;
    public decimal Quantity { get; set; }
    public int UnitsRequested { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    [MaxLength(3)] public string Currency { get; set; } = "USD";
    public DateTime? StartUtc { get; set; }
    public DateTime? EndUtc { get; set; }
    public int? GuestCount { get; set; }
    [MaxLength(150)] public string? ContactName { get; set; }
    [MaxLength(200)] public string? ContactEmail { get; set; }
    [MaxLength(50)] public string? ContactPhone { get; set; }
    [MaxLength(2000)] public string? Notes { get; set; }
}
