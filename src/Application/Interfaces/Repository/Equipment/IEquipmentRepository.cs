using Bookazone.Domain.Entities.Equipment;
using EquipmentEntity = Bookazone.Domain.Entities.Equipment.Equipment;

namespace Bookazone.Application.Interfaces.Repository.Equipment;

public interface IEquipmentRepository : IGenericRepository<EquipmentEntity>
{
}
