namespace Bookazone.Application.DTOs;

public class VwUserPermission
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PermissionId { get; set; }
    public string? PermissionName { get; set; }
    public string? PermissionSlug { get; set; }
    
}