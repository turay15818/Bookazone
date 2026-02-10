using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Vehicles;
using Bookazone.Domain.Entities.Vehicles;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Vehicles;

public class VehicleMediaRepository(
    BookazoneDbContext context,
    ILogger<VehicleMediaRepository> logger)
    : BaseRepository<VehicleMedia>(context, logger), IVehicleMediaRepository
{
    public async Task<List<VehicleMedia>> GetByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await Context.VehicleMedia
            .AsNoTracking()
            .Include(m => m.FkStoredFile)
            .Where(m => m.FkVehicleId == vehicleId && m.Active && !m.Deleted)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);
    }
}
