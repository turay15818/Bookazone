using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Others;

namespace Bookazone.Domain.Entities.Sports;

public class TenantSportMedia : UserBaseEntity
{
    public Guid FkTenantSportTypeId { get; set; }
    public TenantSportType FkTenantSportType { get; set; } = null!;
    public Guid FkStoredFileId { get; set; }
    public StoredFile FkStoredFile { get; set; } = null!;
    public bool IsPrimary { get; set; } = false;
    public int SortOrder { get; set; } = 0;
}
