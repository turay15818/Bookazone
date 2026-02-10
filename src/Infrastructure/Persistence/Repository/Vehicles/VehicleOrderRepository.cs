using Bookazone.Application.Interfaces.Repository.Vehicles;
using Bookazone.Domain.Entities.Vehicles;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Vehicles;

public class VehicleOrderRepository(
    BookazoneDbContext context,
    ILogger<VehicleOrderRepository> logger)
    : BaseRepository<VehicleOrder>(context, logger), IVehicleOrderRepository
{
}
