using Bookazone.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Events;

public class EventCategorySubscriptionConfiguration : IEntityTypeConfiguration<EventCategorySubscription>
{
    public void Configure(EntityTypeBuilder<EventCategorySubscription> builder)
    {
        builder.ToTable("EventCategorySubscriptions", "events");
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.FkEventCategory)
            .WithMany()
            .HasForeignKey(e => e.FkEventCategoryId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => e.FkEventCategoryId);
        builder.HasIndex(e => e.FkUserId);
        builder.HasIndex(e => new { e.FkUserId, e.FkEventCategoryId }).IsUnique();
        builder.Property(e => e.ReceivePush).HasDefaultValue(true);
        builder.Property(e => e.ReceiveEmail).HasDefaultValue(true);
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
