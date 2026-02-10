namespace Bookazone.Api.Controllers.V1.Config.Request;

public class OtpRequest : BaseRequest
{
    public string? Username { get; set; }
    public string? Otp { get; set; }
}



public class OtpValidationRequest
{
    public string? Username { get; set; }
    public string? Reference { get; set; }
    public string? Otp { get; set; }
    public string? Token { get; set; }
    public DeviceRequest? DeviceRequest { get; set; }
    public LocationRequest? Location { get; set; }
}


public class LocationRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
