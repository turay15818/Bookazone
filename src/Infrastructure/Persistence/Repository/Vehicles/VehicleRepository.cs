using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Vehicles;
using Bookazone.Domain.Entities.Vehicles;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Vehicles;

public class VehicleRepository(
    BookazoneDbContext context,
    ILogger<VehicleRepository> logger)
    : BaseRepository<Vehicle>(context, logger), IVehicleRepository
{
    public async Task<List<Vehicle>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await Context.Vehicles
            .AsNoTracking()
            .Where(v => v.FkTenantId == tenantId && v.Active && !v.Deleted)
            .ToListAsync(cancellationToken);
    }
}
