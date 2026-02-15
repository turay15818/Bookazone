using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Domain.Entities.Tenant;
using Bookazone.Domain.Enums;

namespace Bookazone.Domain.Entities.Reviews;

public class Review : UserBaseEntity
{
    public Guid FkTenantId { get; set; }
    public Tenants FkTenant { get; set; } = null!;
    public Guid FkCustomerId { get; set; }
    public Users FkCustomer { get; set; } = null!;
    public ReviewTargetType TargetType { get; set; }
    public Guid TargetId { get; set; }
    public ReviewSourceType SourceType { get; set; }
    public Guid SourceId { get; set; }
    public int Rating { get; set; }
    [MaxLength(150)] public string? Title { get; set; }
    [MaxLength(2000)] public string? Body { get; set; }
    public bool IsVerified { get; set; } = true;
    public ReviewStatus Status { get; set; } = ReviewStatus.Approved;
    [MaxLength(500)] public string? ModerationNotes { get; set; }
    [MaxLength(2000)] public string? Reply { get; set; }
    public DateTime? RepliedAtUtc { get; set; }
    public Guid? ReplyByUserId { get; set; }
    public Users? ReplyByUser { get; set; }
    public ICollection<ReviewAspectRating> AspectRatings { get; set; } = new List<ReviewAspectRating>();
}
