using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Rentals;
using Bookazone.Domain.Entities.Rentals;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Rentals;

public class RentalMediaRepository(
    BookazoneDbContext context,
    ILogger<RentalMediaRepository> logger)
    : BaseRepository<RentalMedia>(context, logger), IRentalMediaRepository
{
    public Task<List<RentalMedia>> GetByRentalAsync(Guid rentalId, CancellationToken cancellationToken = default)
    {
        return Context.Set<RentalMedia>()
            .Include(m => m.FkStoredFile)
            .Where(m => m.FkRentalId == rentalId && m.Active && !m.Deleted)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);
    }
}
