using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Equipment;

public class EquipmentPolicy : UserBaseEntity
{
    public Guid FkEquipmentId { get; set; }
    public Equipment FkEquipment { get; set; } = null!;
    [MaxLength(150)] public string Title { get; set; } = string.Empty;
    [MaxLength(4000)] public string? Body { get; set; }
    public int SortOrder { get; set; }
}
