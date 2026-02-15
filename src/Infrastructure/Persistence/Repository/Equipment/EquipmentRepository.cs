using Bookazone.Application.Interfaces.Repository.Equipment;
using Bookazone.Domain.Entities.Equipment;
using EquipmentEntity = Bookazone.Domain.Entities.Equipment.Equipment;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Equipment;

public class EquipmentRepository(
    BookazoneDbContext context,
    ILogger<EquipmentRepository> logger)
    : BaseRepository<EquipmentEntity>(context, logger), IEquipmentRepository
{
}
