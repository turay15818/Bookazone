using Bookazone.Domain.Entities.Sports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Sports;

public class SportResourcePricingRuleConfiguration : IEntityTypeConfiguration<SportResourcePricingRule>
{
    public void Configure(EntityTypeBuilder<SportResourcePricingRule> builder)
    {
        builder.ToTable("SportResourcePricingRules", "sports");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Currency).HasMaxLength(3).IsRequired();
        builder.HasOne(e => e.FkSportResource)
            .WithMany()
            .HasForeignKey(e => e.FkSportResourceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => new { e.FkSportResourceId, e.DayOfWeek, e.StartTime, e.EndTime });
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
