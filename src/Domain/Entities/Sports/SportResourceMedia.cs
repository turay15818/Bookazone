using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Others;

namespace Bookazone.Domain.Entities.Sports;

public class SportResourceMedia : UserBaseEntity
{
    public Guid FkSportResourceId { get; set; }
    public SportResource FkSportResource { get; set; } = null!;
    public Guid FkStoredFileId { get; set; }
    public StoredFile FkStoredFile { get; set; } = null!;
    public bool IsPrimary { get; set; } = false;
    public int SortOrder { get; set; } = 0;
}
