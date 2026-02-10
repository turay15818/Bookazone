using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Events;
using Bookazone.Domain.Entities.Events;

namespace Bookazone.Application.Interfaces.Services.Events;

public interface IEventManagementService
{
    Task<ApiResult> GetTenantEventsAsync(
        List<EventStatus>? statuses = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        Guid? categoryId = null,
        string? search = null,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default);

    Task<ApiResult> GetEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<ApiResult> CreateEventAsync(EventCreateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> UpdateEventAsync(EventUpdateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> PublishEventAsync(EventPublishRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> CancelEventAsync(EventCancelRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> DeleteEventAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResult> UpsertEventVenueAsync(Guid eventId, EventVenueUpsertRequest request, CancellationToken cancellationToken = default);

    Task<ApiResult> UpsertEventTicketTypesAsync(
        Guid eventId,
        List<EventTicketTypeUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpsertEventScheduleAsync(
        Guid eventId,
        List<EventScheduleItemUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpsertEventPoliciesAsync(
        Guid eventId,
        List<EventPolicyUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpsertEventMediaAsync(
        Guid eventId,
        List<EventMediaUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default);
}
