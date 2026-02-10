using Bookazone.Domain.Entities.Others;

namespace Bookazone.Application.Interfaces.Repository.Others;

public interface IStoredFileRepository
{
    Task<StoredFile> AddAsync(StoredFile file, CancellationToken cancellationToken = default);
    Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(StoredFile file, CancellationToken cancellationToken = default);
    Task DeleteAsync(StoredFile file, CancellationToken cancellationToken = default);
}