using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Reviews;

public class ReviewAspectRating : BaseEntity
{
    public Guid FkReviewId { get; set; }
    public Review FkReview { get; set; } = null!;
    [MaxLength(100)] public string Label { get; set; } = null!;
    public int Score { get; set; }
    public int SortOrder { get; set; }
}
