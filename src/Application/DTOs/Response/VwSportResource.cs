namespace Bookazone.Application.DTOs.Response;

public class VwSportResource
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TenantSportTypeId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int? Capacity { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
