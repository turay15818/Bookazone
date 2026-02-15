using Bookazone.Application.Interfaces.Repository.Rentals;
using Bookazone.Domain.Entities.Rentals;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Rentals;

public class RentalRepository(
    BookazoneDbContext context,
    ILogger<RentalRepository> logger)
    : BaseRepository<Rental>(context, logger), IRentalRepository
{
}
