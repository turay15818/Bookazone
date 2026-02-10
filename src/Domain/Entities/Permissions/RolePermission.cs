using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Permissions;

public class RolePermission : UserBaseEntity
{
    public Guid FkRoleId { get; set; } 
    public Role? FkRole { get; set; } 
    public Guid FkPermissionId { get; set; } 
    public Permission? FkPermission { get; set; } 

}
