using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Equipment;

public class EquipmentSpec : UserBaseEntity
{
    public Guid FkEquipmentId { get; set; }
    public Equipment FkEquipment { get; set; } = null!;
    [MaxLength(150)] public string Label { get; set; } = string.Empty;
    [MaxLength(500)] public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
