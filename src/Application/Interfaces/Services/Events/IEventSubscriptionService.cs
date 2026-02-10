using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Events;

namespace Bookazone.Application.Interfaces.Services.Events;

public interface IEventSubscriptionService
{
    Task<ApiResult> GetMySubscriptionsAsync(CancellationToken cancellationToken = default);
    Task<ApiResult> UpsertSubscriptionAsync(
        EventCategorySubscriptionUpsertRequest request,
        CancellationToken cancellationToken = default);
    Task<ApiResult> UpsertSubscriptionsAsync(
        List<EventCategorySubscriptionUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default);
}
