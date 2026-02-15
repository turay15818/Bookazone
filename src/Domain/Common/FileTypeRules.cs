using Bookazone.Domain.Enums;

namespace Bookazone.Domain.Common;

public static class FileTypeRules
{
    public static readonly Dictionary<FileCategory, string[]> AllowedMimeTypes = new()
        {
            { FileCategory.ProfileImage, new[] { "image/jpeg", "image/png", "image/webp" } },
            { FileCategory.Document, new[] { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" } },
            { FileCategory.VehicleMedia, new[] { "image/jpeg", "image/png", "image/webp" } },
            { FileCategory.RentalMedia, new[] { "image/jpeg", "image/png", "image/webp" } },
            { FileCategory.EquipmentMedia, new[] { "image/jpeg", "image/png", "image/webp" } }
        };

    public static readonly Dictionary<FileCategory, long> MaxFileSizes =
        new()
        {
            { FileCategory.ProfileImage, 2 * 1024 * 1024 }, // 2MB
            { FileCategory.Document, 10 * 1024 * 1024 },    // 10MB
            { FileCategory.VehicleMedia, 5 * 1024 * 1024 }, // 5MB
            { FileCategory.RentalMedia, 5 * 1024 * 1024 },  // 5MB
            { FileCategory.EquipmentMedia, 5 * 1024 * 1024 } // 5MB
        };
}
