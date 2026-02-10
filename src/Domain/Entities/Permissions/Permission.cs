using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Permissions;

public class Permission : UserBaseEntity
{
    public bool IsSystem { get; set; }
    [Required, MaxLength(100)]public required string Name { get; set; }
    [Required, MaxLength(150)]public required string Slug { get; set; }
    [MaxLength(500)]public string? Description { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

}