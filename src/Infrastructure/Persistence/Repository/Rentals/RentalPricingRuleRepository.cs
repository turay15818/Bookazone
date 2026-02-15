using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Rentals;
using Bookazone.Domain.Entities.Rentals;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Rentals;

public class RentalPricingRuleRepository(
    BookazoneDbContext context,
    ILogger<RentalPricingRuleRepository> logger)
    : BaseRepository<RentalPricingRule>(context, logger), IRentalPricingRuleRepository
{
    public Task<List<RentalPricingRule>> GetByRentalAsync(Guid rentalId, CancellationToken cancellationToken = default)
    {
        return Context.Set<RentalPricingRule>()
            .Where(r => r.FkRentalId == rentalId && r.Active && !r.Deleted)
            .OrderBy(r => r.Unit)
            .ToListAsync(cancellationToken);
    }
}
