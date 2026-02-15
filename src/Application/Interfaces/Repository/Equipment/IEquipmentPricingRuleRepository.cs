using Bookazone.Domain.Entities.Equipment;

namespace Bookazone.Application.Interfaces.Repository.Equipment;

public interface IEquipmentPricingRuleRepository : IGenericRepository<EquipmentPricingRule>
{
    Task<List<EquipmentPricingRule>> GetByEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken = default);
}
