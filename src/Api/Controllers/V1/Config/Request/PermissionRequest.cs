namespace Bookazone.Api.Controllers.V1.Config.Request;

public class PermissionRequest
{
    public Guid FkRole { get; set; }
public List<MutiplePermissionRequest>? Permissions { get; set; }
}

public class MutiplePermissionRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
}