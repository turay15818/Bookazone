using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Others;

namespace Bookazone.Domain.Entities.Equipment;

public class EquipmentMedia : UserBaseEntity
{
    public Guid FkEquipmentId { get; set; }
    public Equipment FkEquipment { get; set; } = null!;
    public Guid FkStoredFileId { get; set; }
    public StoredFile FkStoredFile { get; set; } = null!;
    public bool IsCover { get; set; } = false;
    public int SortOrder { get; set; }
}
