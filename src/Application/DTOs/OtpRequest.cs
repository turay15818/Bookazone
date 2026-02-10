using System.ComponentModel.DataAnnotations;
using Bookazone.Api.Controllers.V1.Config.Request;

namespace Bookazone.Application.DTOs;

public class OtpRequest
{
    [MaxLength(300)]
    public string? Email { get; set; }
    [MaxLength(50)]
    public string? Phone { get; set; }
    [MaxLength(300)]
    public string? Purpose { get; set; }
    public DeviceRequest? DeviceRequest { get; set; }
}


public class OtpGenerateRequest
{
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Purpose { get; set; }
}


public class BookazoneOtpGenerate : BaseRequest
{
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Purpose { get; set; }
    public bool IsTokenBased { get; set; }
    public Guid? UserId { get; set; }
    
    
    public Dictionary<string, string>? ExtraClaims { get; set; }

}

public class BookazoneOtpResponse
{
    public string? Reference { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Token { get; set; }
    public string? Otp { get; set; }
    public DateTime? ExpiredDate { get; set; }
}

public class BookazoneOtpValidate
{
    public string? Reference { get; set; }
    public string? Otp { get; set; }
}

public class BookazoneOtpResult
{
    public int? Status { get; set; }
    public string? Message { get; set; }
    public dynamic? Data { get; set; }
}