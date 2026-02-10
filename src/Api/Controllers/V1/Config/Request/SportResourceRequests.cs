using System.ComponentModel.DataAnnotations;

namespace Bookazone.Api.Controllers.V1.Config.Request;

public class SportResourceCreateRequest
{
    [Required] public Guid TenantId { get; set; }
    [Required] public Guid TenantSportTypeId { get; set; }
    [Required, MaxLength(150)] public string Name { get; set; } = null!;
    [MaxLength(1000)] public string? Description { get; set; }
    public int? Capacity { get; set; }
    [MaxLength(300)] public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class SportResourceUpdateRequest
{
    [Required] public Guid Id { get; set; }
    [Required] public Guid TenantId { get; set; }
    [Required] public Guid TenantSportTypeId { get; set; }
    [Required, MaxLength(150)] public string Name { get; set; } = null!;
    [MaxLength(1000)] public string? Description { get; set; }
    public int? Capacity { get; set; }
    [MaxLength(300)] public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
