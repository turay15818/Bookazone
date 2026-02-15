using Bookazone.Domain.Entities.Rentals;

namespace Bookazone.Application.Interfaces.Repository.Rentals;

public interface IRentalPricingRuleRepository : IGenericRepository<RentalPricingRule>
{
    Task<List<RentalPricingRule>> GetByRentalAsync(Guid rentalId, CancellationToken cancellationToken = default);
}
