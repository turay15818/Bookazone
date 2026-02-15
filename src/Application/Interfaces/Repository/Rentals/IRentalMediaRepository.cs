using Bookazone.Domain.Entities.Rentals;

namespace Bookazone.Application.Interfaces.Repository.Rentals;

public interface IRentalMediaRepository : IGenericRepository<RentalMedia>
{
    Task<List<RentalMedia>> GetByRentalAsync(Guid rentalId, CancellationToken cancellationToken = default);
}
