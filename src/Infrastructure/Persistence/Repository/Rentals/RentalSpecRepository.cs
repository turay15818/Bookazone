using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Rentals;
using Bookazone.Domain.Entities.Rentals;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Rentals;

public class RentalSpecRepository(
    BookazoneDbContext context,
    ILogger<RentalSpecRepository> logger)
    : BaseRepository<RentalSpec>(context, logger), IRentalSpecRepository
{
    public Task<List<RentalSpec>> GetByRentalAsync(Guid rentalId, CancellationToken cancellationToken = default)
    {
        return Context.Set<RentalSpec>()
            .Where(r => r.FkRentalId == rentalId && r.Active && !r.Deleted)
            .OrderBy(r => r.SortOrder)
            .ToListAsync(cancellationToken);
    }
}
