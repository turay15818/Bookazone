using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Vehicles;
using Bookazone.Domain.Entities.Vehicles;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Vehicles;

public class VehiclePolicyRepository(
    BookazoneDbContext context,
    ILogger<VehiclePolicyRepository> logger)
    : BaseRepository<VehiclePolicy>(context, logger), IVehiclePolicyRepository
{
    public async Task<List<VehiclePolicy>> GetByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await Context.VehiclePolicies
            .AsNoTracking()
            .Where(p => p.FkVehicleId == vehicleId && p.Active && !p.Deleted)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(cancellationToken);
    }
}
