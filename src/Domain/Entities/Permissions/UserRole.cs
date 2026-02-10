using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Domain.Entities.Tenant;

namespace Bookazone.Domain.Entities.Permissions;

public class UserRole : UserBaseEntity
{
    public new Guid FkUserId { get; set; }
    public new Users? FkUser { get; set; }
    public Guid FkRoleId { get; set; }
    public Role? FkRole { get; set; }
    public Tenants? Tenant { get; set; }

}

