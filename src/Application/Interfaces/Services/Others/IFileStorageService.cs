using Bookazone.Domain.Entities.Others;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Interfaces.Services.Others;

public interface IFileStorageService
{
    Task<StoredFile> UploadAsync(IFormFile file, Guid tenantId, Guid userId, FileCategory category, CancellationToken cancellationToken = default);
    Task<StoredFile> UpdateAsync(Guid fileId, IFormFile newFile, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid fileId);
}