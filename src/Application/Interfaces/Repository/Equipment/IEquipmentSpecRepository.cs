using Bookazone.Domain.Entities.Equipment;

namespace Bookazone.Application.Interfaces.Repository.Equipment;

public interface IEquipmentSpecRepository : IGenericRepository<EquipmentSpec>
{
    Task<List<EquipmentSpec>> GetByEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken = default);
}
