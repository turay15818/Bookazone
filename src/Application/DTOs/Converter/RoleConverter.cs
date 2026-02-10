using Bookazone.Domain.Entities.Permissions;


namespace Bookazone.Application.DTOs.Converter;

public static class RoleConverter
{
    public static RoleDto ToDto(Role role)
    {
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            Permissions = role.RolePermissions
                .Where(rp => rp.FkPermission != null && rp.Active ==true &&rp.Deleted != true)
                .Select(rp => new PermissionDto
                {
                    Id = rp.FkPermission!.Id,
                    Name = rp.FkPermission.Name,
                    Description = rp.FkPermission.Description
                })
                .ToList()
        };
    }
}



public class RoleDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<PermissionDto> Permissions { get; set; } = new();
}


public class PermissionDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; } 
}
