namespace Bookazone.Api.Controllers.V1.Config.Request;

public class ResendVerificationRequest
{
    public string? Username { get; set; }
    public bool UseToken { get; set; } = true; 
    public DeviceRequest? DeviceRequest { get; set; }
}
