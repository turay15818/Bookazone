using Bookazone.Domain.Entities.Equipment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Equipment;

public class EquipmentPricingRuleConfiguration : IEntityTypeConfiguration<EquipmentPricingRule>
{
    public void Configure(EntityTypeBuilder<EquipmentPricingRule> builder)
    {
        builder.ToTable("EquipmentPricingRules", "equipment");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Currency).HasMaxLength(3).IsRequired();

        builder.HasOne(e => e.FkEquipment)
            .WithMany()
            .HasForeignKey(e => e.FkEquipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.FkEquipmentId);
        builder.HasIndex(e => new { e.FkEquipmentId, e.Unit });
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
