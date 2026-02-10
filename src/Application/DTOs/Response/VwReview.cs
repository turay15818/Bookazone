using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Response;

public class VwReviewListItem
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public ReviewTargetType TargetType { get; set; }
    public Guid TargetId { get; set; }
    public ReviewSourceType SourceType { get; set; }
    public Guid SourceId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerAvatarUrl { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public bool IsVerified { get; set; }
    public ReviewStatus Status { get; set; }
    public string? Reply { get; set; }
    public DateTime? ReplyAtUtc { get; set; }
    public DateTime? CreatedAtUtc { get; set; }
}

public class VwReviewDetail : VwReviewListItem
{
    public string? ModerationNotes { get; set; }
    public List<VwReviewAspectRating> Aspects { get; set; } = new();
}

public class VwReviewAspectRating
{
    public string Label { get; set; } = string.Empty;
    public int Score { get; set; }
    public int SortOrder { get; set; }
}

public class VwReviewSummary
{
    public ReviewTargetType TargetType { get; set; }
    public Guid TargetId { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public List<VwReviewRatingCount> Ratings { get; set; } = new();
}

public class VwReviewRatingCount
{
    public int Rating { get; set; }
    public int Count { get; set; }
}
