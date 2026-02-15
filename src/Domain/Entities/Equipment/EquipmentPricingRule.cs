using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;
using Bookazone.Domain.Enums;

namespace Bookazone.Domain.Entities.Equipment;

public class EquipmentPricingRule : UserBaseEntity
{
    public Guid FkEquipmentId { get; set; }
    public Equipment FkEquipment { get; set; } = null!;
    public EquipmentPricingUnit Unit { get; set; } = EquipmentPricingUnit.Day;
    public decimal PriceAmount { get; set; }
    [MaxLength(3)] public string Currency { get; set; } = "USD";
    public decimal? MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
    public bool IsPrimary { get; set; }
}
