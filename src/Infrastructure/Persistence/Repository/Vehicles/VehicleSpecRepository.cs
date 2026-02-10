using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Vehicles;
using Bookazone.Domain.Entities.Vehicles;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Vehicles;

public class VehicleSpecRepository(
    BookazoneDbContext context,
    ILogger<VehicleSpecRepository> logger)
    : BaseRepository<VehicleSpec>(context, logger), IVehicleSpecRepository
{
    public async Task<List<VehicleSpec>> GetByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await Context.VehicleSpecs
            .AsNoTracking()
            .Where(s => s.FkVehicleId == vehicleId && s.Active && !s.Deleted)
            .OrderBy(s => s.SortOrder)
            .ToListAsync(cancellationToken);
    }
}
