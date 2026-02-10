using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Bookazone.Api.Controllers.V1.Config.Request;

public class UsernameCheckRequest : BaseRequest
{
    [MaxLength(300)]
    public string? Username { get; set; }

    [AllowNull]
    public new DeviceRequest? DeviceRequest { get; set; }
}