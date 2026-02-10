using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Bookazone.Api.Controllers.V1.Config.Request;

public class DeviceRequest
{
    [NotNull] [MaxLength(500)] public string? Name { get; set; }
    [NotNull] [MaxLength(500)] public string? Identifier { get; set; }
    [NotNull] [MaxLength(3000)] public string? Version { get; set; }
    [NotNull] [MaxLength(100)] public string? AppVersion { get; set; }
    [NotNull][MaxLength(500)] public string? System { get; set; }
    [MaxLength(50)] public string? Os { get; set; }
    public bool? Verified { get; set; } = false;
}

