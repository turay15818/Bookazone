using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Sports;
using Bookazone.Domain.Entities.Sports;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Sports;

public class TenantSportMediaRepository(
    BookazoneDbContext context,
    ILogger<TenantSportMediaRepository> logger)
    : BaseRepository<TenantSportMedia>(context, logger), ITenantSportMediaRepository
{
    public async Task<List<TenantSportMedia>> GetBySportTypeAsync(Guid tenantSportTypeId, CancellationToken cancellationToken = default)
    {
        return await Context.TenantSportMedia
            .AsNoTracking()
            .Where(m =>
                m.FkTenantSportTypeId == tenantSportTypeId &&
                m.Active &&
                !m.Deleted)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);
    }
}
