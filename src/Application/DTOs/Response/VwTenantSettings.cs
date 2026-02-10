namespace Bookazone.Application.DTOs.Response;

public class VwTenantSettings
{
    public Guid TenantId { get; set; }
    public string? TenantName { get; set; }

    public bool RestrictUsersByPlan { get; set; }
    public string? DefaultTimeZone { get; set; }
    public string? Currency { get; set; }
    public DateTime? DateUpdated { get; set; }
}
