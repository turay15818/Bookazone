using Bookazone.Application.Interfaces.Repository.Others;
using Bookazone.Domain.Entities.Others;
using Bookazone.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Bookazone.Infrastructure.Persistence.Repository.Others;

public class StoredFileRepository(BookazoneDbContext context) : IStoredFileRepository
{
    public async Task<StoredFile> AddAsync(StoredFile file, CancellationToken cancellationToken = default)
    {
        context.StoredFiles.Add(file);
        await context.SaveChangesAsync(cancellationToken);
        return file;
    }

    public async Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.StoredFiles
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(StoredFile file, CancellationToken cancellationToken = default)
    {
        context.StoredFiles.Update(file);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(StoredFile file, CancellationToken cancellationToken = default)
    {
        context.StoredFiles.Remove(file);
        await context.SaveChangesAsync(cancellationToken);
    }
}
