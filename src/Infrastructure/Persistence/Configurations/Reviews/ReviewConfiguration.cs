using Bookazone.Domain.Entities.Reviews;
using Bookazone.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Reviews;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews", "reviews", t =>
            t.HasCheckConstraint("CK_Reviews_Rating", "\"Rating\" >= 1 AND \"Rating\" <= 5"));
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title).HasMaxLength(150);
        builder.Property(e => e.Body).HasMaxLength(2000);
        builder.Property(e => e.ModerationNotes).HasMaxLength(500);
        builder.Property(e => e.Reply).HasMaxLength(2000);
        builder.Property(e => e.IsVerified).HasDefaultValue(true);
        builder.Property(e => e.Status).HasDefaultValue(ReviewStatus.Pending);

        builder.HasOne(e => e.FkTenant)
            .WithMany()
            .HasForeignKey(e => e.FkTenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.FkCustomer)
            .WithMany()
            .HasForeignKey(e => e.FkCustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ReplyByUser)
            .WithMany()
            .HasForeignKey(e => e.ReplyByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.AspectRatings)
            .WithOne(a => a.FkReview)
            .HasForeignKey(a => a.FkReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.FkTenantId);
        builder.HasIndex(e => e.FkCustomerId);
        builder.HasIndex(e => new { e.TargetType, e.TargetId });
        builder.HasIndex(e => new { e.Status, e.TargetType });
        builder.HasIndex(e => new { e.SourceType, e.SourceId }).IsUnique();
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
