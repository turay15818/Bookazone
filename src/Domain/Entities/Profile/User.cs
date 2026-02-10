using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Permissions;
using Bookazone.Domain.Entities.Tenant;

namespace Bookazone.Domain.Entities.Profile;

public class Users : BaseEntity
{
    [MaxLength(50)]
    public string? Firstname { get; set; }
    [MaxLength(50)]
    public string? Lastname { get; set; }
    [MaxLength(150)]
    public string? Username { get; set; }
    [MaxLength(50)]
    public string? Email { get; set; }
    [MaxLength(50)]
    public string? Phone { get; set; }
    [MaxLength(300)]
    public string? Address { get; set; }
    [MaxLength(15)]
    public string? Gender { get; set; }
    [MaxLength(2500)]
    public string? ProfileImage { get; set; }
    [MaxLength(2000)]
    public string? FcmToken { get; set; }
    public int? TryCount { get; set; } = 0;
    public bool? EmailVerified { get; set; } = false;
    public bool? PhoneVerified { get; set; } = false;
    public bool? Locked { get; set; } = false;
    public bool? Frozen { get; set; } = false;
    public bool EnforcePasswordExpiry { get; set; } = false;
    public int PasswordExpiryDays { get; set; } = 90; 
    public bool? PasswordExpired { get; set; } = false;
    public bool? Disable2Fa { get; set; } = false;
    public DateTime? Birthdate { get; set; }
    public DateTime? LastAuthenticated { get; set; }
    public DateTime? LastTryCount { get; set; }
    public bool? AppStatusException { get; set; } = false;
    public double? Longitude { get; set; }
    public double? Latitude { get; set; }
    
    public Guid? FkTenantId { get; set; }
    public Tenants? FkTenant { get; set; }
    public ICollection<UserPassword> Passwords { get; set; } = new List<UserPassword>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    public ICollection<Device> Devices { get; set; } = new List<Device>();
    public ICollection<AuthProvider> AuthProviders { get; set; } = new List<AuthProvider>();

}