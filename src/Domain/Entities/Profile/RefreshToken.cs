using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Profile;

public class RefreshToken : UserBaseEntity
{
    [Required]
    public new Guid FkUserId { get; set; }
    [Required]
    public Guid FkDeviceId { get; set; }
    public new Users? FkUser { get; set; }
    public Device? FkDevice { get; set; }
    [MaxLength(5000)]
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddDays(14);
    public bool Revoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }
    [MaxLength(500)]public string? RevokeReason { get; set; }
    
    [MaxLength(5000)]public string? ReplacedByToken { get; set; }
}
