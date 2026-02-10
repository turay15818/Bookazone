namespace Bookazone.Application.DTOs;

public class UserAccessDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public List<string> DirectPermissions { get; set; } = [];
    public List<string> RolePermissions { get; set; } = [];
    public List<string> Roles { get; set; } = [];
}
