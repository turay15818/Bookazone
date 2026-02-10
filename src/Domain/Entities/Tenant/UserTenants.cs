using Bookazone.Domain.Common;
using Bookazone.Domain.Enums;

namespace Bookazone.Domain.Entities.Tenant;

public class UserTenant : UserBaseEntity
{
    public Guid TenantId { get; set; }
    public Tenants FkTenant { get; set; } = null!;
    public TenantRole Role { get; set; }
    // Customer | Staff | Admin
}
