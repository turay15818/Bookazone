using System.ComponentModel.DataAnnotations;

namespace Bookazone.Api.Controllers.V1.Config.Request;

public class PasswordResetRequest : BaseRequest
{
    [MaxLength(500)]
    public string? Username { get; set; }
    public bool UseToken { get; set; } = true;
}



public class VerifyResetRequest
{
    public string? Username { get; set; }
    public string? Reference { get; set; }
    public string? Otp { get; set; }
    public string? Token { get; set; }
}