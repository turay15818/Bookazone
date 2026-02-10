using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Sports;
using Bookazone.Domain.Entities.Sports;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Sports;

public class SportResourcePricingRuleRepository(
    BookazoneDbContext context,
    ILogger<SportResourcePricingRuleRepository> logger)
    : BaseRepository<SportResourcePricingRule>(context, logger), ISportResourcePricingRuleRepository
{
    public async Task<List<SportResourcePricingRule>> GetByResourceAsync(Guid resourceId, CancellationToken cancellationToken = default)
    {
        return await Context.SportResourcePricingRules
            .AsNoTracking()
            .Where(r =>
                r.FkSportResourceId == resourceId &&
                r.Active &&
                !r.Deleted)
            .OrderBy(r => r.DayOfWeek)
            .ThenBy(r => r.StartTime)
            .ToListAsync(cancellationToken);
    }
}
