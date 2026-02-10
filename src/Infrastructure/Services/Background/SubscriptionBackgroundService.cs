
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Bookazone.Infrastructure.Persistence;
using Bookazone.Infrastructure.Persistence.DbContext;

namespace Bookazone.Infrastructure.Services.Background;

public class SubscriptionBackgroundService(
    IServiceProvider serviceProvider,
    ILogger<SubscriptionBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Subscription background job started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<BookazoneDbContext>();
                var emailService = scope.ServiceProvider.GetRequiredService<SubscriptionEmailService>();
                await ProcessExpiringSubscriptionsAsync(dbContext, emailService, stoppingToken);
                await ProcessExpiredSubscriptionsAsync(dbContext, emailService, stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine("----------------------------------------");
                Console.WriteLine(ex);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
                Console.WriteLine(ex.InnerException?.StackTrace);
                Console.WriteLine(ex.InnerException?.Message);
                Console.WriteLine("----------------------------------------");
                logger.LogError(ex, "Error during subscription background job execution");
            }
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }

        logger.LogInformation("Subscription background job stopped.");
    }

    private async Task ProcessExpiringSubscriptionsAsync(BookazoneDbContext db, SubscriptionEmailService emailService, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var threshold = now.AddDays(3);

        var expiring = await db.TenantSubscriptions
            .Include(s => s.FkTenant)
            .Include(s => s.FkPlan)
            .Where(s => s.IsActive && !s.Deleted && s.EndDate > now && s.EndDate <= threshold)
            .ToListAsync(ct);

        if (!expiring.Any())
        {
            logger.LogInformation("No subscriptions expiring within 3 days.");
            return;
        }

        logger.LogInformation("Found {Count} expiring subscriptions.", expiring.Count);

        foreach (var sub in expiring)
        {
            try
            {
                await emailService.SendSubscriptionExpiringSoonEmailAsync(sub.FkTenant, sub);
                logger.LogInformation("Reminder sent to tenant {TenantName}", sub.FkTenant?.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending expiry reminder to {Tenant}", sub.FkTenant?.Name);
            }
        }
    }

    private async Task ProcessExpiredSubscriptionsAsync(BookazoneDbContext db, SubscriptionEmailService emailService, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var expired = await db.TenantSubscriptions
            .Include(s => s.FkTenant)
            .Include(s => s.FkPlan)
            .Where(s => s.IsActive && !s.Deleted && s.EndDate <= now)
            .ToListAsync(ct);

        if (!expired.Any())
        {
            logger.LogInformation("No expired subscriptions found.");
            return;
        }

        logger.LogInformation("Found {Count} expired subscriptions to deactivate.", expired.Count);

        foreach (var sub in expired)
        {
            try
            {
                sub.IsActive = false;
                sub.Deleted = true;
                sub.DateUpdated = DateTime.UtcNow;

                await emailService.SendSubscriptionExpiredEmailAsync(sub.FkTenant, sub);
                logger.LogInformation("Deactivated and notified tenant {TenantName}", sub.FkTenant?.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deactivating subscription for {Tenant}", sub.FkTenant?.Name);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
