using Bookazone.Application.Interfaces.Repository.Equipment;
using Bookazone.Domain.Entities.Equipment;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Equipment;

public class EquipmentOrderRepository(
    BookazoneDbContext context,
    ILogger<EquipmentOrderRepository> logger)
    : BaseRepository<EquipmentOrder>(context, logger), IEquipmentOrderRepository
{
}
