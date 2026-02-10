using Bookazone.Domain.Entities.Sports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Sports;

public class SportResourceBlackoutConfiguration : IEntityTypeConfiguration<SportResourceBlackout>
{
    public void Configure(EntityTypeBuilder<SportResourceBlackout> builder)
    {
        builder.ToTable("SportResourceBlackouts", "sports", t => t.HasCheckConstraint("CK_SportResourceBlackouts_TimeRange", "\"StartUtc\" < \"EndUtc\""));
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Reason).HasMaxLength(250);
        builder.HasOne(e => e.FkSportResource)
            .WithMany()
            .HasForeignKey(e => e.FkSportResourceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => new { e.FkSportResourceId, e.StartUtc, e.EndUtc });
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
