using Bookazone.Domain.Entities.Equipment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Equipment;

public class EquipmentPolicyConfiguration : IEntityTypeConfiguration<EquipmentPolicy>
{
    public void Configure(EntityTypeBuilder<EquipmentPolicy> builder)
    {
        builder.ToTable("EquipmentPolicies", "equipment");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).HasMaxLength(150).IsRequired();
        builder.Property(e => e.Body).HasMaxLength(4000);

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
