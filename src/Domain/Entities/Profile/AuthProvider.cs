using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Profile;

public class AuthProvider : UserBaseEntity
{
    [MaxLength(50)] public string Provider { get; set; } = null!; // Google | Facebook | Apple
    [MaxLength(200)] public string ProviderUserId { get; set; } = null!;
    [MaxLength(500)] public string? Email { get; set; }
    [MaxLength(2000)] public string? PictureUrl { get; set; }
    public bool EmailVerified { get; set; } = false;

    public bool IsPrimary { get; set; } = false;
}