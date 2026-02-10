using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Response;
using Bookazone.Application.Interfaces.Repository.Subscription;
using Bookazone.Domain.Entities.Subscription;
using Bookazone.Infrastructure.Persistence.DbContext;


namespace Bookazone.Infrastructure.Persistence.Repository.Subscription;

public class SubscriptionPlanRepository(
    BookazoneDbContext context,
    ILogger<SubscriptionPlanRepository> logger
) : ISubscriptionPlanRepository
{
    public async Task<IEnumerable<VwSubscriptionPlan>> AllAsync()
    {
        var plans = await context
            .SubscriptionPlan.AsNoTracking()
            .Where(p => !p.Deleted)
            .OrderBy(p => p.PricePerPeriod)
            .ToListAsync();

        return plans.Select(p => new VwSubscriptionPlan
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            PricePerPeriod = p.PricePerPeriod,
            Period = p.Period,
            UserLimit = p.UserLimit,
            IsPublic = p.IsPublic,
            IsActive = p.IsActive,
        });
    }

    public async Task<VwSubscriptionPlan?> FindAsync(Guid id)
    {
        var plan = await context
            .SubscriptionPlan.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
        return plan == null
            ? null
            : new VwSubscriptionPlan
            {
                Id = plan.Id,
                Name = plan.Name,
                Description = plan.Description,
                PricePerPeriod = plan.PricePerPeriod,
                Period = plan.Period,
                UserLimit = plan.UserLimit,
                IsPublic = plan.IsPublic,
                IsActive = plan.IsActive,
            };
    }

    public async Task<VwSubscriptionPlan?> FindByNameAsync(string name)
    {
        var plan = await context
            .SubscriptionPlan.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Name.ToLower() == name.ToLower() && !p.Deleted);
        return plan == null
            ? null
            : new VwSubscriptionPlan
            {
                Id = plan.Id,
                Name = plan.Name,
                Description = plan.Description,
                PricePerPeriod = plan.PricePerPeriod,
                Period = plan.Period,
                UserLimit = plan.UserLimit,
                IsPublic = plan.IsPublic,
                IsActive = plan.IsActive,
            };
    }

    public async Task<SubscriptionPlan> CreateAsync(SubscriptionPlan plan)
    {
        plan.DateCreated = DateTime.UtcNow;
        plan.Active = true;
        plan.Deleted = false;
        context.SubscriptionPlan.Add(plan);
        await context.SaveChangesAsync();
        logger.LogInformation("Created subscription plan {Name}", plan.Name);
        return plan;
    }

    public async Task<SubscriptionPlan> UpdateAsync(SubscriptionPlan plan)
    {
        plan.DateUpdated = DateTime.UtcNow;
        context.SubscriptionPlan.Update(plan);
        await context.SaveChangesAsync();
        return plan;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var plan = await context.SubscriptionPlan.FirstOrDefaultAsync(p => p.Id == id);
        if (plan == null)
            throw new Except(ErrorHttp.NotFound);
        plan.Deleted = true;
        plan.DateDeleted = DateTime.UtcNow;
        context.SubscriptionPlan.Update(plan);
        await context.SaveChangesAsync();
        return true;
    }
}
