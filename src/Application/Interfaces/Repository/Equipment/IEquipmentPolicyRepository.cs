using Bookazone.Domain.Entities.Equipment;

namespace Bookazone.Application.Interfaces.Repository.Equipment;

public interface IEquipmentPolicyRepository : IGenericRepository<EquipmentPolicy>
{
    Task<List<EquipmentPolicy>> GetByEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken = default);
}
