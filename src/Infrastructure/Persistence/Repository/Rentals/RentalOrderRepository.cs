using Bookazone.Application.Interfaces.Repository.Rentals;
using Bookazone.Domain.Entities.Rentals;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Rentals;

public class RentalOrderRepository(
    BookazoneDbContext context,
    ILogger<RentalOrderRepository> logger)
    : BaseRepository<RentalOrder>(context, logger), IRentalOrderRepository
{
}
