using Bookazone.Domain.Entities.Equipment;

namespace Bookazone.Application.Interfaces.Repository.Equipment;

public interface IEquipmentMediaRepository : IGenericRepository<EquipmentMedia>
{
    Task<List<EquipmentMedia>> GetByEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken = default);
}
