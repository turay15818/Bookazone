namespace Bookazone.Application.DTOs.Response;

public class VwSportResourcePublic
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int? Capacity { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = null!;
    public string? TenantAddress { get; set; }
    public string? TenantCity { get; set; }
    public string? TenantState { get; set; }
    public string? TenantCountry { get; set; }
    public string? TenantLogo { get; set; }
    public bool TenantVerified { get; set; }
    public Guid SportTypeId { get; set; }
    public string SportTypeName { get; set; } = null!;
    public List<VwSportResourceAvailability> AvailabilityHours { get; set; } = new();
}
