using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Rentals;

public class RentalOrderCreateRequest
{
    public Guid RentalId { get; set; }
    public RentalPricingUnit PricingUnit { get; set; }
    public decimal? Quantity { get; set; }
    public int? UnitsRequested { get; set; }
    public DateTime? StartUtc { get; set; }
    public DateTime? EndUtc { get; set; }
    public int? GuestCount { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Notes { get; set; }
}

public class RentalOrderStatusUpdateRequest
{
    public Guid OrderId { get; set; }
    public RentalOrderStatus Status { get; set; }
}

public class RentalOrderCancelRequest
{
    public Guid OrderId { get; set; }
}
