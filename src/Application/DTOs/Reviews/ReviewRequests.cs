using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Reviews;

public class ReviewCreateRequest
{
    public ReviewTargetType TargetType { get; set; }
    public Guid TargetId { get; set; }
    public ReviewSourceType SourceType { get; set; }
    public Guid SourceId { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public List<ReviewAspectRequest>? Aspects { get; set; }
}

public class ReviewAspectRequest
{
    public string Label { get; set; } = string.Empty;
    public int Score { get; set; }
    public int? SortOrder { get; set; }
}

public class ReviewStatusUpdateRequest
{
    public Guid ReviewId { get; set; }
    public ReviewStatus Status { get; set; }
    public string? ModerationNotes { get; set; }
}

public class ReviewReplyRequest
{
    public Guid ReviewId { get; set; }
    public string Reply { get; set; } = string.Empty;
}
