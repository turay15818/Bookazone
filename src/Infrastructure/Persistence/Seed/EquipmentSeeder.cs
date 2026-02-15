using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Booking;
using Bookazone.Domain.Entities.Equipment;
using Bookazone.Domain.Entities.Others;
using Bookazone.Domain.Enums;
using Bookazone.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Bookazone.Infrastructure.Persistence.Seed;

public static class EquipmentSeeder
{
    private const string PlaceholderPngBase64 =
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR4nGNgYAAAAAMAASsJTYQAAAAASUVORK5CYII=";

    private sealed record EquipmentSeed(
        string Title,
        string Category,
        string Description,
        decimal Price,
        int UnitsAvailable,
        string FileName);

    public static async Task SeedAsync(
        BookazoneDbContext context,
        ILogger logger,
        string contentRootPath,
        Guid tenantId,
        Guid userId)
    {
        var tenant = await context.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId && !t.Deleted);

        if (tenant == null)
        {
            logger.LogWarning("Equipment seeding skipped: tenant {TenantId} not found or inactive.", tenantId);
            return;
        }

        var bookingCategoryCode = BookingCategoryTypeMapper.ToCode(BookingCategoryType.EquipmentRental);
        var bookingCategory = await context.BookingCategories
            .FirstOrDefaultAsync(c => c.Code == bookingCategoryCode && !c.Deleted);

        var createdUserEntities = new List<UserBaseEntity>();

        if (bookingCategory == null)
        {
            bookingCategory = new BookingCategory
            {
                Name = "Equipment Rental",
                Code = bookingCategoryCode,
                Description = "Equipment rental category.",
                DefaultBookingMode = BookingMode.Approval,
                Active = true,
                Deleted = false
            };
            context.BookingCategories.Add(bookingCategory);
            createdUserEntities.Add(bookingCategory);
        }
        else if (!bookingCategory.Active)
        {
            bookingCategory.Active = true;
            context.BookingCategories.Update(bookingCategory);
        }

        var tenantCategory = await context.TenantBookingCategories
            .FirstOrDefaultAsync(tbc =>
                tbc.FkTenantId == tenant.Id &&
                tbc.FkBookingCategoryId == bookingCategory.Id &&
                !tbc.Deleted);

        if (tenantCategory == null)
        {
            tenantCategory = new TenantBookingCategory
            {
                FkTenantId = tenant.Id,
                FkBookingCategoryId = bookingCategory.Id,
                IsEnabled = true,
                BookingModeOverride = BookingMode.Approval,
                Active = true,
                Deleted = false
            };
            context.TenantBookingCategories.Add(tenantCategory);
            createdUserEntities.Add(tenantCategory);
        }
        else
        {
            var shouldUpdate = false;
            if (!tenantCategory.IsEnabled)
            {
                tenantCategory.IsEnabled = true;
                shouldUpdate = true;
            }

            if (!tenantCategory.Active)
            {
                tenantCategory.Active = true;
                shouldUpdate = true;
            }

            if (shouldUpdate)
                context.TenantBookingCategories.Update(tenantCategory);
        }

        var seeds = new List<EquipmentSeed>
        {
            new("Banquet Chairs", "Chairs", "Padded banquet chairs for events and receptions.", 2.50m, 200, "chairs.png"),
            new("White Canopy Tent", "Canopy", "Outdoor canopy tent for weddings and gatherings.", 120m, 10, "canopy.png"),
            new("Round Tables (6ft)", "Tables", "Round banquet tables seating 8-10 guests.", 8m, 50, "tables.png"),
            new("PA Sound System", "Sound", "Portable sound system with speakers and mixer.", 75m, 5, "sound-system.png"),
            new("LED Event Lighting Kit", "Lighting", "LED uplight kit for stages and venues.", 45m, 8, "lighting.png")
        };

        var relativeFolder = FilePathBuilder.BuildTenantPath(tenant.Id, FileCategory.EquipmentMedia);
        var absoluteFolder = Path.Combine(contentRootPath, relativeFolder);
        Directory.CreateDirectory(absoluteFolder);
        var coverAssignments = new List<(Equipment equipment, EquipmentMedia media)>();

        foreach (var seed in seeds)
        {
            var exists = await context.Equipments.AsNoTracking()
                .AnyAsync(e => e.FkTenantId == tenant.Id && e.Title == seed.Title && !e.Deleted);
            if (exists)
                continue;

            var absolutePath = Path.Combine(absoluteFolder, seed.FileName);
            var relativePath = Path.Combine(relativeFolder, seed.FileName);

            EnsureImageFile(absolutePath);

            var storedFile = new StoredFile
            {
                FkTenantId = tenant.Id,
                OriginalFileName = seed.FileName,
                StoredFileName = seed.FileName,
                RelativePath = relativePath,
                ContentType = "image/png",
                SizeInBytes = new FileInfo(absolutePath).Length,
                Category = FileCategory.EquipmentMedia,
                Active = true,
                Deleted = false
            };

            var equipment = new Equipment
            {
                FkTenantId = tenant.Id,
                Title = seed.Title,
                Description = seed.Description,
                Category = seed.Category,
                City = tenant.City,
                Address = tenant.Address,
                UnitsAvailable = seed.UnitsAvailable,
                DeliveryAvailable = true,
                PickupAvailable = true,
                MinBookingDuration = 1,
                MaxBookingDuration = 30,
                BookingDurationUnit = EquipmentPricingUnit.Day,
                BookingMode = BookingMode.Approval,
                Status = EquipmentStatus.Published,
                PriceFrom = seed.Price,
                Currency = "USD",
                Active = true,
                Deleted = false
            };

            var media = new EquipmentMedia
            {
                FkEquipmentId = equipment.Id,
                FkStoredFileId = storedFile.Id,
                IsCover = true,
                SortOrder = 1,
                Active = true,
                Deleted = false
            };

            var pricingRule = new EquipmentPricingRule
            {
                FkEquipmentId = equipment.Id,
                Unit = EquipmentPricingUnit.Day,
                PriceAmount = seed.Price,
                Currency = equipment.Currency ?? "USD",
                MinQuantity = 1,
                IsPrimary = true,
                Active = true,
                Deleted = false
            };

            context.StoredFiles.Add(storedFile);
            context.Equipments.Add(equipment);
            context.EquipmentMedia.Add(media);
            context.EquipmentPricingRules.Add(pricingRule);

            createdUserEntities.Add(storedFile);
            createdUserEntities.Add(equipment);
            createdUserEntities.Add(media);
            createdUserEntities.Add(pricingRule);
            coverAssignments.Add((equipment, media));
        }

        await context.SaveChangesAsync();

        if (createdUserEntities.Count == 0)
            return;

        foreach (var (equipment, media) in coverAssignments)
            equipment.FkCoverMediaId = media.Id;

        foreach (var entity in createdUserEntities)
            entity.FkUserId = userId;

        await context.SaveChangesAsync();
    }

    private static void EnsureImageFile(string absolutePath)
    {
        if (File.Exists(absolutePath))
            return;

        var bytes = Convert.FromBase64String(PlaceholderPngBase64);
        File.WriteAllBytes(absolutePath, bytes);
    }
}
