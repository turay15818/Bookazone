using Bookazone.Application.Interfaces.Repository;
using Bookazone.Domain.Entities.Sports;

namespace Bookazone.Application.Interfaces.Repository.Sports;

public interface ISportResourcePricingRuleRepository : IGenericRepository<SportResourcePricingRule>
{
    Task<List<SportResourcePricingRule>> GetByResourceAsync(Guid resourceId, CancellationToken cancellationToken = default);
}
