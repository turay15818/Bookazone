using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Events;

namespace Bookazone.Application.Interfaces.Services.Events;

public interface IEventDiscoveryService
{
    Task<ApiResult> GetEventCategoriesAsync(CancellationToken cancellationToken = default);
    Task<ApiResult> GetEventsAsync(EventSearchRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> GetEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<ApiResult> GetSimilarEventsAsync(Guid eventId, int? take = null, CancellationToken cancellationToken = default);
}
