using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Sports;
using Bookazone.Domain.Entities.Sports;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Sports;

public class TenantSportTypeRepository(
    BookazoneDbContext context,
    ILogger<TenantSportTypeRepository> logger)
    : BaseRepository<TenantSportType>(context, logger), ITenantSportTypeRepository
{
    public async Task<List<TenantSportType>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await Context.TenantSportTypes
            .AsNoTracking()
            .Where(t =>
                t.FkTenantId == tenantId &&
                t.Active &&
                !t.Deleted)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<TenantSportType?> GetByTenantAndNameAsync(Guid tenantId, string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var normalized = name.Trim().ToLower();

        return await Context.TenantSportTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.FkTenantId == tenantId &&
                t.Name.ToLower() == normalized &&
                t.Active &&
                !t.Deleted,
                cancellationToken);
    }
}
