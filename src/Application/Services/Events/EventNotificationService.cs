using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Repository.Events;
using Bookazone.Application.Interfaces.Services.Events;
using Bookazone.Domain.Entities.Events;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Infrastructure.Services;

namespace Bookazone.Application.Services.Events;

public class EventNotificationService(
    IEventRepository eventRepository,
    IEventTicketTypeRepository eventTicketTypeRepository,
    IEventCategorySubscriptionRepository eventCategorySubscriptionRepository,
    FirebaseNotificationService firebaseNotificationService,
    IEmailService emailService,
    ILogger<EventNotificationService> logger)
    : IEventNotificationService
{
    public async Task NotifyEventPublishedAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        try
        {
            var evt = await eventRepository.Query()
                .AsNoTracking()
                .Include(e => e.FkEventCategory)
                .Include(e => e.FkTenant)
                .Include(e => e.FkVenue)
                .Include(e => e.FkCoverMedia)
                .FirstOrDefaultAsync(e =>
                    e.Id == eventId &&
                    e.Active &&
                    !e.Deleted,
                    cancellationToken);

            if (evt == null || evt.Status != EventStatus.Published)
                return;

            var (priceFrom, currency) = await ResolvePriceAsync(evt.Id, evt.PriceFrom, eventTicketTypeRepository, cancellationToken);

            var subscriptions = await eventCategorySubscriptionRepository.Query()
                .AsNoTracking()
                .Include(s => s.FkUser)
                .ThenInclude(u => u!.Devices)
                .Where(s =>
                    s.FkEventCategoryId == evt.FkEventCategoryId &&
                    s.Active &&
                    !s.Deleted &&
                    s.FkUser != null &&
                    s.FkUser.Active &&
                    !s.FkUser.Deleted)
                .ToListAsync(cancellationToken);

            if (subscriptions.Count == 0)
                return;

            var categoryName = evt.FkEventCategory?.Name ?? "Event";
            var startUtc = evt.StartUtc;
            var dateLabel = startUtc.ToString("ddd, MMM dd HH:mm 'UTC'");
            var title = $"New {categoryName} event";
            var body = $"{evt.Title} on {dateLabel}.";
            var data = new Dictionary<string, string>
            {
                { "type", "event_published" },
                { "eventId", evt.Id.ToString() },
                { "categoryId", evt.FkEventCategoryId.ToString() },
                { "tenantId", evt.FkTenantId.ToString() }
            };

            await SendPushNotificationsAsync(subscriptions, title, body, data);
            await SendEmailNotificationsAsync(subscriptions, evt, categoryName, priceFrom, currency);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error notifying subscribers for event {EventId}", eventId);
        }
    }

    private async Task SendPushNotificationsAsync(
        List<EventCategorySubscription> subscriptions,
        string title,
        string body,
        Dictionary<string, string> data)
    {
        var pushSubscriptions = subscriptions
            .Where(s => s.ReceivePush)
            .ToList();

        if (pushSubscriptions.Count == 0)
            return;

        var deviceTokens = pushSubscriptions
            .SelectMany(s => s.FkUser?.Devices ?? Enumerable.Empty<Device>())
            .Where(d =>
                d.Active &&
                !d.Deleted &&
                d.Verified == true &&
                !string.IsNullOrWhiteSpace(d.NotificationToken))
            .Select(d => d.NotificationToken!)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (deviceTokens.Count > 0)
            await firebaseNotificationService.PushToTokensAsync(deviceTokens, title, body, data);
    }

    private async Task SendEmailNotificationsAsync(
        List<EventCategorySubscription> subscriptions,
        Event evt,
        string categoryName,
        decimal? priceFrom,
        string? currency)
    {
        var emailRecipients = subscriptions
            .Where(s => s.ReceiveEmail)
            .Select(s => s.FkUser)
            .Where(u => u != null && !string.IsNullOrWhiteSpace(u.Email))
            .GroupBy(u => u!.Email!.Trim().ToLowerInvariant())
            .Select(g => g.First()!)
            .ToList();

        if (emailRecipients.Count == 0)
            return;

        var subject = $"New {categoryName} event: {evt.Title}";
        var eventUrl = $"{Consts.AppSetting.FrontendUrl.TrimEnd('/')}/events/{evt.Id}";

        foreach (var user in emailRecipients)
        {
            var displayName = BuildDisplayName(user);
            var body = emailService.GenerateEventAnnouncementEmailHtml(
                displayName,
                evt.Title,
                categoryName,
                evt.StartUtc,
                evt.FkVenue?.Name,
                evt.City,
                priceFrom,
                currency,
                evt.FkCoverMedia?.Url,
                eventUrl);

            await emailService.SendEmailAsync(user.Email!, subject, body, true);
        }
    }

    private static async Task<(decimal? PriceFrom, string? Currency)> ResolvePriceAsync(
        Guid eventId,
        decimal? eventPriceFrom,
        IEventTicketTypeRepository eventTicketTypeRepository,
        CancellationToken cancellationToken)
    {
        if (eventPriceFrom.HasValue)
        {
            var currency = await eventTicketTypeRepository.Query()
                .AsNoTracking()
                .Where(t => t.FkEventId == eventId && t.Active && !t.Deleted)
                .OrderBy(t => t.PriceAmount)
                .Select(t => t.Currency)
                .FirstOrDefaultAsync(cancellationToken);

            return (eventPriceFrom, currency);
        }

        var ticket = await eventTicketTypeRepository.Query()
            .AsNoTracking()
            .Where(t => t.FkEventId == eventId && t.Active && !t.Deleted)
            .OrderBy(t => t.PriceAmount)
            .Select(t => new { t.PriceAmount, t.Currency })
            .FirstOrDefaultAsync(cancellationToken);

        return ticket == null
            ? (null, null)
            : (ticket.PriceAmount, ticket.Currency);
    }

    private static string BuildDisplayName(Users user)
    {
        var fullName = $"{user.Firstname} {user.Lastname}".Trim();
        if (!string.IsNullOrWhiteSpace(fullName))
            return fullName;

        if (!string.IsNullOrWhiteSpace(user.Username))
            return user.Username;

        return "there";
    }
}
