using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Tenant;
using Bookazone.Domain.Enums;

namespace Bookazone.Domain.Entities.Others;

public class StoredFile : UserBaseEntity
{
    public Guid? FkTenantId { get; set; }
    public Tenants? FkTenant { get; set; } = null!;
   [MaxLength(20000)] public string OriginalFileName { get; set; } = null!;
   [MaxLength(20000)] public string StoredFileName { get; set; } = null!;
   [MaxLength(20000)] public string RelativePath { get; set; } = null!;
   [MaxLength(20000)] public string ContentType { get; set; } = null!;
    public long SizeInBytes { get; set; }
    public FileCategory Category { get; set; }
}
