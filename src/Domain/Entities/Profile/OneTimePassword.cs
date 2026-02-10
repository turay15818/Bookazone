using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Profile;

public class OneTimePassword : UserBaseEntity
{
    [MaxLength(100)]
    public string? Reference { get; set; }
    [MaxLength(50)]
    public string? Otp { get; set; }
    [MaxLength(300)]
    public string? Email { get; set; }
    [MaxLength(50)]
    public string? Phone { get; set; }
    [MaxLength(300)]
    public string? Purpose { get; set; }
    public bool? TransmitEmail { get; set; }
    
    [MaxLength(300)] public string? TransmitStatus { get; set; }
    [MaxLength(300)] public string? ErrorCode { get; set; }
    [MaxLength(300)] public string? ErrorMessage { get; set; }
    
    public bool? TransmitPhone { get; set; }
    public bool Used { get; set; }
    public DateTime? ExpiredDate { get; set; }
    public DateTime? TransmitEmailDate { get; set; }
    public DateTime? TransmitPhoneDate { get; set; }
    public DateTime? UsedTimeDate { get; set; }
    
    public new Guid? FkUserId { get; set; }
    public new Users? FkUser { get; set; }
    
    
    [MaxLength(5120)]
    public string? Token { get; set; } 
    public bool IsTokenBased { get; set; } 
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    [MaxLength(500)]public string? DeviceIdentifier { get; set; }
}