namespace Bookazone.Application.DTOs.Response;

public class VwTenantPublic
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? Logo { get; set; }
    public bool IsVerified { get; set; }
}
