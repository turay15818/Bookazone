using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Vehicles;
using Bookazone.Domain.Entities.Vehicles;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Vehicles;

public class VehiclePricingRuleRepository(
    BookazoneDbContext context,
    ILogger<VehiclePricingRuleRepository> logger)
    : BaseRepository<VehiclePricingRule>(context, logger), IVehiclePricingRuleRepository
{
    public async Task<List<VehiclePricingRule>> GetByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await Context.VehiclePricingRules
            .AsNoTracking()
            .Where(r => r.FkVehicleId == vehicleId && r.Active && !r.Deleted)
            .ToListAsync(cancellationToken);
    }
}
