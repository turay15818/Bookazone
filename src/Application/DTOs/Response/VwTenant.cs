namespace Bookazone.Application.DTOs.Response;

public class VwTenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool Active { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? DateCreated { get; set; }
}