using Bookazone.Domain.Entities.Reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Reviews;

public class ReviewAspectRatingConfiguration : IEntityTypeConfiguration<ReviewAspectRating>
{
    public void Configure(EntityTypeBuilder<ReviewAspectRating> builder)
    {
        builder.ToTable("ReviewAspectRatings", "reviews", t =>
            t.HasCheckConstraint("CK_ReviewAspectRatings_Score", "\"Score\" >= 1 AND \"Score\" <= 5"));
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Label).HasMaxLength(100).IsRequired();

        builder.HasOne(e => e.FkReview)
            .WithMany(r => r.AspectRatings)
            .HasForeignKey(e => e.FkReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.FkReviewId);
        builder.HasIndex(e => new { e.FkReviewId, e.SortOrder });
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
