using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;
using Bookazone.Domain.Enums;

namespace Bookazone.Domain.Entities.Rentals;

public class RentalPricingRule : UserBaseEntity
{
    public Guid FkRentalId { get; set; }
    public Rental FkRental { get; set; } = null!;
    public RentalPricingUnit Unit { get; set; } = RentalPricingUnit.Day;
    public decimal PriceAmount { get; set; }
    [MaxLength(3)] public string Currency { get; set; } = "USD";
    public decimal? MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
    public bool IsPrimary { get; set; }
}
