using Bookazone.Application.Interfaces.Context;
using Bookazone.Application.Interfaces.Repository.Others;
using Bookazone.Application.Interfaces.Services.Others;
using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Others;
using Bookazone.Domain.Enums;

namespace Bookazone.Infrastructure.Services.Files;

public class LocalFileStorageService(IStoredFileRepository storedFileRepository, IUserContext userContext) : IFileStorageService
{
    public async Task<StoredFile> UploadAsync(IFormFile file, Guid tenantId, Guid userId, FileCategory category, CancellationToken cancellationToken = default)
    {
        ValidateFile(file, category);
        var folderPath = FilePathBuilder.BuildTenantPath(tenantId, category);
        Directory.CreateDirectory(folderPath);
        var storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var fullPath = Path.Combine(folderPath, storedFileName);
        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var storedFile = new StoredFile
        {
            FkTenantId = userContext.TenantId ?? null,
            FkUserId = userContext.UserId,
            OriginalFileName = file.FileName,
            StoredFileName = storedFileName,
            RelativePath = fullPath,
            ContentType = file.ContentType,
            SizeInBytes = file.Length,
            Category = category
        };
        return await storedFileRepository.AddAsync(storedFile, cancellationToken);
    }

    public async Task<StoredFile> UpdateAsync(Guid fileId, IFormFile newFile, CancellationToken cancellationToken = default)
    {
        var existing = await storedFileRepository.GetByIdAsync(fileId, cancellationToken) ?? throw new Exception("File not found");
        ValidateFile(newFile, existing.Category);
        if (File.Exists(existing.RelativePath)) File.Delete(existing.RelativePath);
        var newFileName = $"{Guid.NewGuid()}{Path.GetExtension(newFile.FileName)}";
        var newPath = Path.Combine(Path.GetDirectoryName(existing.RelativePath)!, newFileName);
        await using (var stream = new FileStream(newPath, FileMode.Create))
        {
            await newFile.CopyToAsync(stream, cancellationToken);
        }
        existing.StoredFileName = newFileName;
        existing.RelativePath = newPath;
        existing.SizeInBytes = newFile.Length;
        existing.ContentType = newFile.ContentType;
        await storedFileRepository.UpdateAsync(existing, cancellationToken);
        return existing;
    }

    public async Task DeleteAsync(Guid fileId)
    {
        var file = await storedFileRepository.GetByIdAsync(fileId) ?? throw new Exception("File not found");
        if (File.Exists(file.RelativePath)) File.Delete(file.RelativePath);
        await storedFileRepository.DeleteAsync(file);
    }

    private static void ValidateFile(IFormFile file, FileCategory category)
    {
        if (!FileTypeRules.AllowedMimeTypes[category].Contains(file.ContentType)) throw new Exception("File type not allowed");
        if (file.Length > FileTypeRules.MaxFileSizes[category]) throw new Exception("File too large");
    }
}