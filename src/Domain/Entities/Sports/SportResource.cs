using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Tenant;

namespace Bookazone.Domain.Entities.Sports;

public class SportResource : UserBaseEntity
{
    public Guid FkTenantId { get; set; }
    public Tenants FkTenant { get; set; } = null!;
    public Guid FkTenantSportTypeId { get; set; }
    public TenantSportType FkTenantSportType { get; set; } = null!;
    [MaxLength(150)] public string Name { get; set; } = null!;
    [MaxLength(1000)] public string? Description { get; set; }
    public int? Capacity { get; set; }
    [MaxLength(300)] public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
