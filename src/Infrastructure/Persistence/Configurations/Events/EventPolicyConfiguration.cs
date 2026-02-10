using Bookazone.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Events;

public class EventPolicyConfiguration : IEntityTypeConfiguration<EventPolicy>
{
    public void Configure(EntityTypeBuilder<EventPolicy> builder)
    {
        builder.ToTable("EventPolicies", "events");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).HasMaxLength(150).IsRequired();
        builder.Property(e => e.Body).HasMaxLength(2000);
        builder.HasOne(e => e.FkEvent)
            .WithMany()
            .HasForeignKey(e => e.FkEventId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => e.FkEventId);
        builder.HasIndex(e => new { e.FkEventId, e.SortOrder });
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
