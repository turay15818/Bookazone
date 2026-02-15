using Bookazone.Domain.Entities.Rentals;

namespace Bookazone.Application.Interfaces.Repository.Rentals;

public interface IRentalPolicyRepository : IGenericRepository<RentalPolicy>
{
    Task<List<RentalPolicy>> GetByRentalAsync(Guid rentalId, CancellationToken cancellationToken = default);
}
