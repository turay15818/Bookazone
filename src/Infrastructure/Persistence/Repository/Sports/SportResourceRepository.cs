using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Sports;
using Bookazone.Domain.Entities.Sports;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Sports;

public class SportResourceRepository(
    BookazoneDbContext context,
    ILogger<SportResourceRepository> logger)
    : BaseRepository<SportResource>(context, logger), ISportResourceRepository
{
    public async Task<List<SportResource>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await Context.SportResources
            .AsNoTracking()
            .Where(r =>
                r.FkTenantId == tenantId &&
                r.Active &&
                !r.Deleted)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SportResource>> GetBySportTypeAsync(Guid tenantSportTypeId, CancellationToken cancellationToken = default)
    {
        return await Context.SportResources
            .AsNoTracking()
            .Where(r =>
                r.FkTenantSportTypeId == tenantSportTypeId &&
                r.Active &&
                !r.Deleted)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }
}
