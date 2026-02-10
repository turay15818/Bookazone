
using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Permissions;

public class PermissionGroup : UserBaseEntity
{
    [Required, MaxLength(100)] public string? Name { get; set; }
    [MaxLength(250)] public string? Description { get; set; }
    public ICollection<Permission>? Permission { get; set; }
}
