using Bookazone.Application.DTOs.Response;
using Bookazone.Domain.Entities.Subscription;

namespace Bookazone.Application.Interfaces.Repository.Subscription;

public interface ISubscriptionPlanRepository
{
    Task<IEnumerable<VwSubscriptionPlan>> AllAsync();
    Task<VwSubscriptionPlan?> FindAsync(Guid id);
    Task<VwSubscriptionPlan?> FindByNameAsync(string name);
    Task<SubscriptionPlan> CreateAsync(SubscriptionPlan plan);
    Task<SubscriptionPlan> UpdateAsync(SubscriptionPlan plan);
    Task<bool> DeleteAsync(Guid id);
}