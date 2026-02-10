using Microsoft.EntityFrameworkCore;
using Bookazone.Domain.Entities.Subscription;
using Bookazone.Infrastructure.Persistence.DbContext;

namespace Bookazone.Infrastructure.Persistence.Seed;

public static class SubscriptionPlanSeeder
{
    public static async Task SeedAsync(BookazoneDbContext context, ILogger logger)
    {
        try
        {
            await context.Database.EnsureCreatedAsync();
            if (await context.SubscriptionPlan.AnyAsync())
            {
                logger.LogInformation("✅ Subscription plans already exist, skipping seeding.");
                return;
            }
            logger.LogInformation("🚀 Seeding default subscription plans...");

            var plans = new List<SubscriptionPlan>
            {
                new()
                {
                    Id = new Guid("a9b6f2e0-2a3d-4c67-8a67-5db60e5d0408"),
                    Name = "Free",
                    Description = "Free plan with limited access and basic features.",
                    PricePerPeriod = 0,
                    Period = "monthly",
                    UserLimit = 3,
                    IsPublic = true,
                    IsActive = true,
                    MetadataJson = "{\"features\": [\"Basic Analytics\", \"Limited Users\"]}",
                    DateCreated = DateTime.UtcNow,
                    Active = true,
                    Deleted = false,
                },
                new()
                {
                    Id = new Guid("993c5ff4-d9f8-42a4-9275-65a295f8d60a"),
                    Name = "Pro",
                    Description = "Pro plan with unlimited users and advanced features.",
                    PricePerPeriod = 29.99M,
                    Period = "monthly",
                    UserLimit = null,
                    IsPublic = true,
                    IsActive = true,
                    MetadataJson =
                        "{\"features\": [\"Advanced Reports\", \"Unlimited Users\", \"API Access\"]}",
                    DateCreated = DateTime.UtcNow,
                    Active = true,
                    Deleted = false,
                },
                new()
                {
                    Id = new Guid("b7ed3e5f-1d3a-4dfb-bd55-ec6d9b03cc43"),
                    Name = "Enterprise",
                    Description = "Enterprise plan with full customization and support.",
                    PricePerPeriod = 99.99M,
                    Period = "monthly",
                    UserLimit = null,
                    IsPublic = false,
                    IsActive = true,
                    MetadataJson =
                        "{\"features\": [\"Dedicated Support\", \"Custom SLAs\", \"Integrations\"]}",
                    DateCreated = DateTime.UtcNow,
                    Active = true,
                    Deleted = false,
                },
            };
            foreach (var plan in plans) await UpsertPlanAsync(context, plan);
            await context.SaveChangesAsync();
            logger.LogInformation("🎉 Seeded {Count} subscription plans successfully.", plans.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Failed to seed subscription plans.");
            throw;
        }
    }
    
    
    private static async Task UpsertPlanAsync(BookazoneDbContext context, SubscriptionPlan plan)
    {
        var existing = await context.SubscriptionPlan.FirstOrDefaultAsync(p => p.Name == plan.Name);
        if (existing == null)
        {
            context.SubscriptionPlan.Add(plan);
        }
        else
        {
            existing.Description = plan.Description;
            existing.PricePerPeriod = plan.PricePerPeriod;
            existing.Period = plan.Period;
            existing.UserLimit = plan.UserLimit;
            existing.MetadataJson = plan.MetadataJson;
            existing.IsPublic = plan.IsPublic;
            existing.IsActive = plan.IsActive;
            existing.DateUpdated = DateTime.UtcNow;
            context.SubscriptionPlan.Update(existing);
        }
    }
  
    
    
}


