using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Permissions;

namespace Bookazone.Domain.Entities.Permissions;

public class Role : UserBaseEntity
{
    [Required, MaxLength(100)] public string? Name { get; set; }
    [Required, MaxLength(100)] public string? Slug { get; set; }
    [MaxLength(250)] public string? Description { get; set; }
    public bool IsSystem { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<Permission> Permission { get; set; } = new List<Permission>();
}
