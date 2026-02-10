using System.ComponentModel.DataAnnotations;

namespace Bookazone.Application.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    [MaxLength(300)]
    public string? Username { get; set; }

    [MaxLength(300)]
    public string? Email { get; set; }

    [MaxLength(300)]
    public string? Phone { get; set; }

    [MaxLength(3000)]
    public string? FirstName { get; set; }

    [MaxLength(3000)]
    public string? LastName { get; set; }
    [MaxLength(20)]
    public string? Gender { get; set; }

    [MaxLength(300)]
    public string? ReferralId { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }
    [MaxLength(1500)]
    public string? ProfileImage { get; set; }

    public string? District { get; set; }
    public DateTime? Birthdate { get; set; }
    public bool? EmailVerified { get; set; } = false;
    public bool? PhoneVerified { get; set; } = false;
    public bool? Frozen { get; set; } = false;
    public bool? PermissionUserManagement { get; set; } = false;
    public string? LastAuthenticated { get; set; }

    // Extended fields for profile response
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}




public class UserInviteRequest
{
    [Required, MaxLength(300)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? FirstName { get; set; }

    [MaxLength(300)]
    public string? LastName { get; set; }

    public List<string>? Roles { get; set; } = new();
    public List<string>? Permissions { get; set; } = new();

    public string? RedirectUrl { get; set; } // optional front-end link
}
