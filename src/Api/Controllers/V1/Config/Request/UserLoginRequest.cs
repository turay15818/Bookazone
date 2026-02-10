using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Bookazone.Api.Controllers.V1.Config.Request;

public class UserLoginRequest : BaseRequest
{

    [NotNull]
    [MaxLength(300)]
    [MinLength(1)]
    public string? Username { get; set; }
    

    [MaxLength(300)]
    public string? Phone { get; set; }
    
    [PasswordPropertyText]
    [MaxLength(300)]
    [MinLength(4)]
    public string? Password { get; set; }
    public string? NotificationToken { get; set; }
}

public class RefreshTokenRequest
{
    public string Token { get; set; } = string.Empty;
    public DeviceRequest DeviceRequest { get; set; } = new DeviceRequest();

}
