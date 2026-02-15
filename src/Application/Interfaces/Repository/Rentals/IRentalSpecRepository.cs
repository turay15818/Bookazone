using Bookazone.Domain.Entities.Rentals;

namespace Bookazone.Application.Interfaces.Repository.Rentals;

public interface IRentalSpecRepository : IGenericRepository<RentalSpec>
{
    Task<List<RentalSpec>> GetByRentalAsync(Guid rentalId, CancellationToken cancellationToken = default);
}
