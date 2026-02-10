using System.ComponentModel.DataAnnotations;

namespace Bookazone.Api.Controllers.V1.Config.Request;

public class InviteAcceptRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public DeviceRequest DeviceRequest { get; set; } = new();
}


public class SetPasswordRequest
{
    [Required]
    public string Token { get; set; } = string.Empty; 

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfPassword { get; set; } = string.Empty;

    [Required]
    public DeviceRequest DeviceRequest { get; set; } = new();

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
