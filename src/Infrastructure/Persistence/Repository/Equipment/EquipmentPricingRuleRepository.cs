using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Equipment;
using Bookazone.Domain.Entities.Equipment;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Equipment;

public class EquipmentPricingRuleRepository(
    BookazoneDbContext context,
    ILogger<EquipmentPricingRuleRepository> logger)
    : BaseRepository<EquipmentPricingRule>(context, logger), IEquipmentPricingRuleRepository
{
    public Task<List<EquipmentPricingRule>> GetByEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        return Context.Set<EquipmentPricingRule>()
            .Where(r => r.FkEquipmentId == equipmentId && r.Active && !r.Deleted)
            .OrderBy(r => r.Unit)
            .ToListAsync(cancellationToken);
    }
}
