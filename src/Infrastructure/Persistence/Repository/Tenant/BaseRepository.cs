using Bookazone.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore.Storage;

namespace Bookazone.Infrastructure.Persistence.Repository.Tenant;

public abstract class BaseRepository(BookazoneDbContext context)
{
    protected readonly BookazoneDbContext _context = context;

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }
}