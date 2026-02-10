using System.Diagnostics.CodeAnalysis;

namespace Bookazone.Api.Controllers.V1.Config.Request;

public class BaseRequest
{
    [NotNull] public DeviceRequest? DeviceRequest { get; set; }
    public double? Longitude { get; set; }
    public double? Latitude { get; set; }
}