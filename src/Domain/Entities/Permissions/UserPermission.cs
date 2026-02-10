using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Domain.Entities.Tenant;

namespace Bookazone.Domain.Entities.Permissions;

public class UserPermission : UserBaseEntity
{
    public new Guid FkUserId { get; set; }
    public new Users? FkUser { get; set; } = default!;
    public Guid FkPermissionId { get; set; }
    public Permission? FkPermission { get; set; } = default!;
    public Tenants? FkTenant { get; set; }
    
}
