using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Sports;
using Bookazone.Domain.Entities.Sports;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Sports;

public class SportResourceMediaRepository(
    BookazoneDbContext context,
    ILogger<SportResourceMediaRepository> logger)
    : BaseRepository<SportResourceMedia>(context, logger), ISportResourceMediaRepository
{
    public async Task<List<SportResourceMedia>> GetByResourceAsync(Guid resourceId, CancellationToken cancellationToken = default)
    {
        return await Context.SportResourceMedia
            .AsNoTracking()
            .Where(m =>
                m.FkSportResourceId == resourceId &&
                m.Active &&
                !m.Deleted)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);
    }
}
