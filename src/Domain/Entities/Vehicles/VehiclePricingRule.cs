using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;
using Bookazone.Domain.Enums;

namespace Bookazone.Domain.Entities.Vehicles;

public class VehiclePricingRule : UserBaseEntity
{
    public Guid FkVehicleId { get; set; }
    public Vehicle FkVehicle { get; set; } = null!;
    public VehiclePricingUnit Unit { get; set; } = VehiclePricingUnit.Day;
    public decimal PriceAmount { get; set; }
    [MaxLength(3)] public string Currency { get; set; } = "USD";
    public decimal? MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
    public bool IsPrimary { get; set; }
}
