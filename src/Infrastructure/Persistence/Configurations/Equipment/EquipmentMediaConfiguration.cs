using Bookazone.Domain.Entities.Equipment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Equipment;

public class EquipmentMediaConfiguration : IEntityTypeConfiguration<EquipmentMedia>
{
    public void Configure(EntityTypeBuilder<EquipmentMedia> builder)
    {
        builder.ToTable("EquipmentMedia", "equipment");
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.FkEquipment)
            .WithMany()
            .HasForeignKey(e => e.FkEquipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.FkStoredFile)
            .WithMany()
            .HasForeignKey(e => e.FkStoredFileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.FkEquipmentId, e.SortOrder });
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
