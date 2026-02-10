namespace Bookazone.Api.Controllers.V1.Config.Request;

public class RoleRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
}


public class RolePermissionUpdateRequest
{
    public Guid RoleId { get; set; }
    public List<Guid> PermissionIds { get; set; } = new();
}
