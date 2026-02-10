using Bookazone.Domain.Entities.Subscription;
using Bookazone.Domain.Entities.Tenant;

namespace Bookazone.Infrastructure.Services;

public class SubscriptionEmailService(IEmailService emailService, ILogger<SubscriptionEmailService> logger)
{
    public async Task SendSubscriptionCreatedEmailAsync(Tenants tenant, SubscriptionPlan plan)
    {
        if (string.IsNullOrEmpty(tenant.Email))
        {
            logger.LogWarning("Tenant {TenantName} has no email. Skipping subscription email.", tenant.Name);
            return;
        }

        var subject = $"Welcome to the {plan.Name} plan on Bookazone";
        var body = emailService.GenerateSubscriptionEmailHtml(
            tenant.Name,
            plan.Name,
            plan.Description ?? "Your subscription has been activated successfully.",
            plan.PricePerPeriod.ToString("C"),
            plan.Period
        );

        await emailService.SendEmailAsync(tenant.Email, subject, body);
        logger.LogInformation("✅ Subscription email sent to {Email}", tenant.Email);
    }

    public async Task SendSubscriptionExpiringSoonEmailAsync(Tenants tenant, TenantSubscription subscription)
    {
        if (string.IsNullOrEmpty(tenant.Email))
        {
            logger.LogWarning("Tenant {TenantName} has no email. Skipping expiring soon email.", tenant.Name);
            return;
        }

        var subject = $"Your Bookazone subscription will expire soon";
        var body = emailService.GenerateSubscriptionExpiryReminderHtml(
            tenant.Name,
            subscription.FkPlan?.Name ?? "your plan",
            subscription.EndDate?.ToString("dddd, dd MMMM yyyy") ?? "soon"
        );

        await emailService.SendEmailAsync(tenant.Email, subject, body);
        logger.LogInformation("⚠️ Expiry reminder sent to {Email}", tenant.Email);
    }

    public async Task SendSubscriptionExpiredEmailAsync(Tenants tenant, TenantSubscription subscription)
    {
        if (string.IsNullOrEmpty(tenant.Email))
        {
            logger.LogWarning("Tenant {TenantName} has no email. Skipping expired email.", tenant.Name);
            return;
        }

        var subject = "Your Bookazone subscription has expired";
        var body = emailService.GenerateSubscriptionExpiredEmailHtml(
            tenant.Name,
            subscription.FkPlan?.Name ?? "your plan"
        );

        await emailService.SendEmailAsync(tenant.Email, subject, body);
        logger.LogInformation("❌ Expiration email sent to {Email}", tenant.Email);
    }
}
