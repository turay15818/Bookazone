using System.ComponentModel.DataAnnotations;

namespace Bookazone.Api.Controllers.V1.Config.Request;

public class PasswordResetConfirmRequest : BaseRequest
{
    
    [MaxLength(500)] public string NewPassword { get; set; }
    
    [MaxLength(500)] public string OldPassword { get; set; }

    [MaxLength(500)] public string PasswordRepeat { get; set; }
    
}


public class ChangePasswordRequest : BaseRequest
{
    [MaxLength(500)]
    public string? Password { get; set; }
    [MaxLength(500)]
    public string? ConfPassword { get; set; }
    [MaxLength(5000)]
    public string? Token { get; set; }
}