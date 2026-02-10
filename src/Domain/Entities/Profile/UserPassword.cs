using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Profile;

public class UserPassword : UserBaseEntity
{
    public new Guid FkUserId { get; set; }
    public new Users FkUser { get; set; } = null!;
    [MaxLength(2048)] public string Password { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsCurrent { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
}

