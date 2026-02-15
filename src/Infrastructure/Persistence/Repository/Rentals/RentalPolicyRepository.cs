using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Rentals;
using Bookazone.Domain.Entities.Rentals;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Rentals;

public class RentalPolicyRepository(
    BookazoneDbContext context,
    ILogger<RentalPolicyRepository> logger)
    : BaseRepository<RentalPolicy>(context, logger), IRentalPolicyRepository
{
    public Task<List<RentalPolicy>> GetByRentalAsync(Guid rentalId, CancellationToken cancellationToken = default)
    {
        return Context.Set<RentalPolicy>()
            .Where(r => r.FkRentalId == rentalId && r.Active && !r.Deleted)
            .OrderBy(r => r.SortOrder)
            .ToListAsync(cancellationToken);
    }
}
