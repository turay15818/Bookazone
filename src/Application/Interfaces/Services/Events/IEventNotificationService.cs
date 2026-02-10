namespace Bookazone.Application.Interfaces.Services.Events;

public interface IEventNotificationService
{
    Task NotifyEventPublishedAsync(Guid eventId, CancellationToken cancellationToken = default);
}
