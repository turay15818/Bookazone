using Bookazone.Domain.Entities.Equipment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Equipment;

public class EquipmentSpecConfiguration : IEntityTypeConfiguration<EquipmentSpec>
{
    public void Configure(EntityTypeBuilder<EquipmentSpec> builder)
    {
        builder.ToTable("EquipmentSpecs", "equipment");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Label).HasMaxLength(150).IsRequired();
        builder.Property(e => e.Value).HasMaxLength(500).IsRequired();

        builder.HasOne(e => e.FkEquipment)
            .WithMany()
            .HasForeignKey(e => e.FkEquipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.FkEquipmentId, e.SortOrder });
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
