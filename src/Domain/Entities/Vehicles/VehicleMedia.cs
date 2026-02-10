using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Others;

namespace Bookazone.Domain.Entities.Vehicles;

public class VehicleMedia : UserBaseEntity
{
    public Guid FkVehicleId { get; set; }
    public Vehicle FkVehicle { get; set; } = null!;
    public Guid FkStoredFileId { get; set; }
    public StoredFile FkStoredFile { get; set; } = null!;
    public bool IsCover { get; set; }
    public int SortOrder { get; set; }
}
