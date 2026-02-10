using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Response;
using Bookazone.Application.Interfaces.Repository.Subscription;
using Bookazone.Domain.Entities.Subscription;
using Bookazone.Infrastructure.Persistence.DbContext;


namespace Bookazone.Infrastructure.Persistence.Repository.Subscription;

public class TenantSubscriptionRepository(BookazoneDbContext context, ILogger<TenantSubscriptionRepository> logger)
    : ITenantSubscriptionRepository
{
    public async Task<VwTenantSubscription?> GetActiveForTenantAsync(Guid tenantId)
    {
        var subscription = await context.TenantSubscriptions
            .AsNoTracking()
            .Include(ts => ts.FkTenant)
            .Include(ts => ts.FkPlan)
            .Where(ts => ts.FkTenantId == tenantId && ts.IsActive)
            .OrderByDescending(ts => ts.DateCreated)
            .FirstOrDefaultAsync();

        return subscription == null
            ? null
            : new VwTenantSubscription
            {
                Id = subscription.Id,
                TenantId = subscription.FkTenantId,
                TenantName = subscription.FkTenant?.Name,
                PlanId = subscription.FkPlanId,
                PlanName = subscription.FkPlan?.Name ?? "",
                PlanPrice = subscription.FkPlan?.PricePerPeriod ?? 0,
                PlanPeriod = subscription.FkPlan?.Period ?? "monthly",
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                IsActive = subscription.IsActive,
                IsTrial = subscription.IsTrial,
                TrialEndsAt = subscription.TrialEndsAt,
                ExternalSubscriptionId = subscription.ExternalSubscriptionId
            };
    }

    public async Task<IEnumerable<VwTenantSubscription>> ListByTenantAsync(Guid tenantId)
    {
        var subs = await context.TenantSubscriptions
            .AsNoTracking()
            .Include(ts => ts.FkPlan)
            .Where(ts => ts.FkTenantId == tenantId)
            .OrderByDescending(ts => ts.DateCreated)
            .ToListAsync();

        return subs.Select(s => new VwTenantSubscription
        {
            Id = s.Id,
            TenantId = s.FkTenantId,
            PlanId = s.FkPlanId,
            PlanName = s.FkPlan?.Name ?? "",
            PlanPrice = s.FkPlan?.PricePerPeriod ?? 0,
            PlanPeriod = s.FkPlan?.Period ?? "monthly",
            IsActive = s.IsActive,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            IsTrial = s.IsTrial,
            TrialEndsAt = s.TrialEndsAt
        });
    }

    public async Task<TenantSubscription> CreateAsync(TenantSubscription entity)
    {
        entity.DateCreated = DateTime.UtcNow;
        entity.Active = true;
        entity.Deleted = false;
        context.TenantSubscriptions.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<TenantSubscription> UpdateAsync(TenantSubscription entity)
    {
        entity.DateUpdated = DateTime.UtcNow;
        context.TenantSubscriptions.Update(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeactivateAsync(Guid id, string? deactivatedBy = null)
    {
        var sub = await context.TenantSubscriptions.FirstOrDefaultAsync(t => t.Id == id);
        if (sub == null) throw new Except(ErrorHttp.NotFound);
        sub.IsActive = false;
        sub.Deleted = true;
        sub.DeletedBy = deactivatedBy;
        sub.DateDeleted = DateTime.UtcNow;
        context.TenantSubscriptions.Update(sub);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task CleanupExpiredAsync()
    {
        var expired = await context.TenantSubscriptions
            .Where(ts => ts.IsActive && ts.EndDate < DateTime.UtcNow)
            .ToListAsync();
        foreach (var sub in expired)
        {
            sub.IsActive = false;
            sub.Deleted = true;
            sub.DateDeleted = DateTime.UtcNow;
        }
        if (expired.Any())
        {
            context.TenantSubscriptions.UpdateRange(expired);
            await context.SaveChangesAsync();
            logger.LogInformation("Cleaned up {Count} expired subscriptions", expired.Count);
        }
    }
}
