using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Profile;

public class Device : UserBaseEntity
{
    [MaxLength(150)]
    public string? Slug { get; set; }

    [MaxLength(1000)]
    public string? Name { get; set; }

    [MaxLength(1000)]
    public string? Identifier { get; set; }

    [MaxLength(3000)]
    public string? NotificationToken { get; set; }

    [MaxLength(150)]
    public string? Version { get; set; }

    [MaxLength(100)]
    public string? AppVersion { get; set; }

    [MaxLength(1000)]
    public string? System { get; set; }

    [MaxLength(50)]
    public string? Os { get; set; }

    [MaxLength(5000)]
    public string? BiometricKey { get; set; }

    [MaxLength(100)]
    public string? OtpEmailReference { get; set; }

    [MaxLength(100)]
    public string? OtpPhoneReference { get; set; }

    [MaxLength(1000)]
    public string? MemorySlug { get; set; }

    [MaxLength(15000)]
    public string? CurrentSessionToken { get; set; }
    
    public bool? Verified { get; set; }
    public bool? VerifyByEmail { get; set; }
    public bool? VerifyByPhone { get; set; }
    public bool? Biometric { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public DateTime? DateVerified { get; set; }
    public DateTime? SecurityCountdown { get; set; }
    public DateTime? LastConnect { get; set; }
    public new Users? FkUser { get; set; }
}