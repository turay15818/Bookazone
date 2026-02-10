using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Tenant;

public class TenantSettings : UserBaseEntity
{
    public Guid FkTenantId { get; set; }
    public Tenants FkTenant { get; set; } = null!;
    public bool RestrictUsersByPlan { get; set; } = false; 
    public bool AllowNegativeStock { get; set; } = false;
    [MaxLength(1000000000)]public string? AdditionalJson { get; set; }
    [MaxLength(500)] public string Key { get; set; } = null!;   // e.g., "MaxBookingPerDay"
    [MaxLength(20000)] public string Value { get; set; } = null!; // e.g., "5"
    [MaxLength(1000)] public string? Description { get; set; }   // Optional: explains the setting
    [MaxLength(255)]public string? DefaultTimeZone { get; set; }
    [MaxLength(255)] public string? Currency { get; set; } = "USD";
}
