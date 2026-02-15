using Bookazone.Domain.Entities.Rentals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Rentals;

public class RentalPricingRuleConfiguration : IEntityTypeConfiguration<RentalPricingRule>
{
    public void Configure(EntityTypeBuilder<RentalPricingRule> builder)
    {
        builder.ToTable("RentalPricingRules", "rentals");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Currency).HasMaxLength(3).IsRequired();

        builder.HasOne(e => e.FkRental)
            .WithMany()
            .HasForeignKey(e => e.FkRentalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.FkRentalId);
        builder.HasIndex(e => new { e.FkRentalId, e.Unit });
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
