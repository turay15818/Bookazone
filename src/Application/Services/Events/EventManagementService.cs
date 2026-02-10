using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs;
using Bookazone.Application.DTOs.Converter;
using Bookazone.Application.DTOs.Events;
using Bookazone.Application.DTOs.Response;
using Bookazone.Application.Interfaces.Context;
using Bookazone.Application.Interfaces.Repository.Booking;
using Bookazone.Application.Interfaces.Repository.Events;
using Bookazone.Application.Interfaces.Repository.Tenant;
using Bookazone.Application.Interfaces.Services.Events;
using Bookazone.Domain.Entities.Events;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Services.Events;

public class EventManagementService(
    ITenantRepository tenantRepository,
    ITenantBookingCategoryRepository tenantBookingCategoryRepository,
    IEventRepository eventRepository,
    IEventCategoryRepository eventCategoryRepository,
    IEventVenueRepository eventVenueRepository,
    IEventTicketTypeRepository eventTicketTypeRepository,
    IEventScheduleItemRepository eventScheduleItemRepository,
    IEventPolicyRepository eventPolicyRepository,
    IEventMediaRepository eventMediaRepository,
    IEventOrderItemRepository eventOrderItemRepository,
    IEventNotificationService eventNotificationService,
    IUserContext userContext,
    ILogger<EventManagementService> logger)
    : IEventManagementService
{
    public async Task<ApiResult> GetTenantEventsAsync(
        List<EventStatus>? statuses = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        Guid? categoryId = null,
        string? search = null,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!userContext.TenantId.HasValue)
                return ApiResponse.Error(ErrorHttp.Forbidden,
                    new Exception("User context does not contain a valid tenant id."));

            var tenantId = userContext.TenantId.Value;

            var query = eventRepository.Query()
                .AsNoTracking()
                .Include(e => e.FkTenant)
                .Include(e => e.FkEventCategory)
                .Include(e => e.FkVenue)
                .Include(e => e.FkCoverMedia)
                .Where(e =>
                    e.FkTenantId == tenantId &&
                    e.Active &&
                    !e.Deleted);

            if (categoryId.HasValue)
                query = query.Where(e => e.FkEventCategoryId == categoryId.Value);

            if (statuses is { Count: > 0 })
                query = query.Where(e => statuses.Contains(e.Status));

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLowerInvariant();
                query = query.Where(e =>
                    e.Title.ToLower().Contains(term) ||
                    (e.Subtitle != null && e.Subtitle.ToLower().Contains(term)));
            }

            if (fromUtc.HasValue)
            {
                var from = NormalizeUtc(fromUtc.Value);
                query = query.Where(e => e.EndUtc >= from);
            }

            if (toUtc.HasValue)
            {
                var to = NormalizeUtc(toUtc.Value);
                query = query.Where(e => e.StartUtc <= to);
            }

            var sorted = ApplySorting(query, sortBy, sortDirection);
            var paged = await Paginate<Event>.CreateAsync(sorted, pageIndex, pageSize);

            var pageData = paged.Data ?? new List<Event>();
            var eventIds = pageData.Select(e => e.Id).ToList();
            var priceLookup = await BuildPriceLookupAsync(eventIds, cancellationToken);

            var dto = pageData.Select(e =>
            {
                var priceInfo = ResolvePrice(e, priceLookup);
                return EventConverter.ToListItemDto(
                    e,
                    e.FkEventCategory,
                    e.FkTenant,
                    e.FkVenue,
                    e.FkCoverMedia,
                    priceInfo.PriceFrom,
                    priceInfo.Currency);
            }).ToList();

            var response = new DataPaginate<VwEventListItem>
            {
                Data = dto,
                Pages = paged.Pages,
                PageIndex = paged.PageIndex
            };

            return ApiResponse.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant events");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        try
        {
            var evt = await GetEventWithIncludesAsync(eventId, userContext.TenantId, cancellationToken);
            if (evt == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            var ticketTypes = await eventTicketTypeRepository.GetByEventAsync(eventId, cancellationToken);
            var ticketDtos = await BuildTicketTypeDtosAsync(eventId, ticketTypes, cancellationToken);
            var schedule = await eventScheduleItemRepository.GetByEventAsync(eventId, cancellationToken);
            var policies = await eventPolicyRepository.GetByEventAsync(eventId, cancellationToken);
            var media = await eventMediaRepository.GetByEventAsync(eventId, cancellationToken);

            var priceInfo = ResolvePrice(evt, ticketTypes);

            var detail = EventConverter.ToDetailDto(
                evt,
                evt.FkEventCategory,
                evt.FkTenant,
                evt.FkVenue,
                evt.FkCoverMedia?.Url,
                ticketDtos,
                schedule,
                policies,
                media,
                priceInfo.PriceFrom,
                priceInfo.Currency);

            return ApiResponse.Success(detail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting event {EventId}", eventId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> CreateEventAsync(EventCreateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var validation = ValidateEventRequest(request.Title, request.StartUtc, request.EndUtc);
            if (validation != null)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

            if (request.TenantId == Guid.Empty || request.EventCategoryId == Guid.Empty)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "TenantId and EventCategoryId are required.");

            if (request.Venue != null && string.IsNullOrWhiteSpace(request.Venue.Name))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Venue name is required.");

            if (IsTenantMismatch(request.TenantId))
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User is not authorized for this tenant."));

            if (!await tenantRepository.ExistsAsync(request.TenantId))
                return ApiResponse.Error(ErrorHttp.NotFound);

            var category = await eventCategoryRepository.GetByIdAsync(request.EventCategoryId);
            if (category == null || !category.Active || category.Deleted)
                return ApiResponse.ErrorWithMessage(ErrorHttp.NotFound, "Event category not found or inactive.");

            if (!await TenantSupportsEventsAsync(request.TenantId, cancellationToken))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Tenant is not enabled for events.");

            var evt = new Event
            {
                FkTenantId = request.TenantId,
                FkEventCategoryId = request.EventCategoryId,
                Title = request.Title.Trim(),
                Subtitle = request.Subtitle?.Trim(),
                About = request.About?.Trim(),
                StartUtc = NormalizeUtc(request.StartUtc),
                EndUtc = NormalizeUtc(request.EndUtc),
                TimeZoneId = request.TimeZoneId?.Trim(),
                City = request.City?.Trim(),
                PriceFrom = request.PriceFrom,
                Status = EventStatus.Draft,
                Active = true,
                Deleted = false
            };

            await using var transaction = await eventRepository.BeginTransactionAsync();

            if (request.Venue != null)
            {
                var venue = new EventVenue
                {
                    Name = request.Venue.Name.Trim(),
                    Address = request.Venue.Address?.Trim(),
                    City = request.Venue.City?.Trim(),
                    State = request.Venue.State?.Trim(),
                    Country = request.Venue.Country?.Trim(),
                    Latitude = request.Venue.Latitude,
                    Longitude = request.Venue.Longitude,
                    Active = true,
                    Deleted = false
                };

                var savedVenue = await eventVenueRepository.AddAsync(venue);
                evt.FkVenueId = savedVenue.Id;
            }

            var created = await eventRepository.AddAsync(evt);

            if (request.TicketTypes is { Count: > 0 })
                await CreateTicketTypesAsync(created.Id, request.TicketTypes, cancellationToken);

            if (request.Schedule is { Count: > 0 })
                await CreateScheduleItemsAsync(created.Id, request.Schedule, cancellationToken);

            if (request.Policies is { Count: > 0 })
                await CreatePoliciesAsync(created.Id, request.Policies, cancellationToken);

            if (request.Media is { Count: > 0 })
                await CreateMediaAsync(created, request.Media, cancellationToken);

            await UpdateEventPriceFromAsync(created, force: request.PriceFrom == null, cancellationToken);
            await UpdateEventCoverAsync(created, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return await GetEventAsync(created.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating event for tenant {TenantId}", request.TenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpdateEventAsync(EventUpdateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var validation = ValidateEventRequest(request.Title, request.StartUtc, request.EndUtc);
            if (validation != null)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

            if (request.TenantId == Guid.Empty || request.EventCategoryId == Guid.Empty)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "TenantId and EventCategoryId are required.");

            if (IsTenantMismatch(request.TenantId))
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User is not authorized for this tenant."));

            var evt = await GetEventForUpdateAsync(request.Id, request.TenantId, cancellationToken);
            if (evt == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (evt.Status == EventStatus.Completed)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Completed events cannot be updated.");

            var category = await eventCategoryRepository.GetByIdAsync(request.EventCategoryId);
            if (category == null || !category.Active || category.Deleted)
                return ApiResponse.ErrorWithMessage(ErrorHttp.NotFound, "Event category not found or inactive.");

            evt.FkEventCategoryId = request.EventCategoryId;
            evt.Title = request.Title.Trim();
            evt.Subtitle = request.Subtitle?.Trim();
            evt.About = request.About?.Trim();
            evt.StartUtc = NormalizeUtc(request.StartUtc);
            evt.EndUtc = NormalizeUtc(request.EndUtc);
            evt.TimeZoneId = request.TimeZoneId?.Trim();
            evt.City = request.City?.Trim();
            evt.PriceFrom = request.PriceFrom;

            await eventRepository.UpdateAsync(evt);

            await UpdateEventPriceFromAsync(evt, force: request.PriceFrom == null, cancellationToken);

            return await GetEventAsync(evt.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating event {EventId}", request.Id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> PublishEventAsync(EventPublishRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var evt = await GetEventForUpdateAsync(request.EventId, userContext.TenantId, cancellationToken);
            if (evt == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!await TenantSupportsEventsAsync(evt.FkTenantId, cancellationToken))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Tenant is not enabled for events.");

            if (evt.Status == EventStatus.Cancelled)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Cancelled events cannot be published.");

            if (evt.Status == EventStatus.Published)
                return ApiResponse.Success(true);

            var hasTickets = await eventTicketTypeRepository.Query()
                .AsNoTracking()
                .AnyAsync(t =>
                    t.FkEventId == evt.Id &&
                    t.Active &&
                    !t.Deleted,
                    cancellationToken);

            if (!hasTickets && evt.PriceFrom == null)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Add ticket types or price before publishing.");

            evt.Status = EventStatus.Published;
            await eventRepository.UpdateAsync(evt);

            try
            {
                await eventNotificationService.NotifyEventPublishedAsync(evt.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error notifying subscribers for published event {EventId}", evt.Id);
            }

            return ApiResponse.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing event {EventId}", request.EventId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> CancelEventAsync(EventCancelRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var evt = await GetEventForUpdateAsync(request.EventId, userContext.TenantId, cancellationToken);
            if (evt == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (evt.Status == EventStatus.Completed)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Completed events cannot be cancelled.");

            evt.Status = EventStatus.Cancelled;
            await eventRepository.UpdateAsync(evt);
            return ApiResponse.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cancelling event {EventId}", request.EventId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> DeleteEventAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var evt = await GetEventForUpdateAsync(id, userContext.TenantId, cancellationToken);
            if (evt == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            var deleted = await eventRepository.DeleteAsync(id);
            return deleted
                ? ApiResponse.Success(true)
                : ApiResponse.Error(ErrorHttp.DbDeleteError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting event {EventId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpsertEventVenueAsync(Guid eventId, EventVenueUpsertRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Venue name is required.");

            var evt = await GetEventForUpdateAsync(eventId, userContext.TenantId, cancellationToken);
            if (evt == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            var existingVenue = evt.FkVenueId.HasValue
                ? await eventVenueRepository.GetByIdAsync(evt.FkVenueId.Value)
                : null;

            EventVenue venue;
            if (existingVenue != null)
            {
                venue = existingVenue;
                venue.Name = request.Name.Trim();
                venue.Address = request.Address?.Trim();
                venue.City = request.City?.Trim();
                venue.State = request.State?.Trim();
                venue.Country = request.Country?.Trim();
                venue.Latitude = request.Latitude;
                venue.Longitude = request.Longitude;
                venue.Active = true;
                venue.Deleted = false;

                await eventVenueRepository.UpdateAsync(venue);
            }
            else
            {
                venue = new EventVenue
                {
                    Name = request.Name.Trim(),
                    Address = request.Address?.Trim(),
                    City = request.City?.Trim(),
                    State = request.State?.Trim(),
                    Country = request.Country?.Trim(),
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Active = true,
                    Deleted = false
                };

                var created = await eventVenueRepository.AddAsync(venue);
                evt.FkVenueId = created.Id;
                await eventRepository.UpdateAsync(evt);
            }

            return ApiResponse.Success(EventConverter.ToDto(venue));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting venue for event {EventId}", eventId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpsertEventTicketTypesAsync(
        Guid eventId,
        List<EventTicketTypeUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (requests == null || requests.Count == 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Request list is empty.");

            var evt = await GetEventForUpdateAsync(eventId, userContext.TenantId, cancellationToken);
            if (evt == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            await using IDbContextTransaction? transaction = allOrNothing
                ? await eventTicketTypeRepository.BeginTransactionAsync()
                : null;

            var results = new List<object>();
            var successCount = 0;
            var failureCount = 0;

            foreach (var request in requests)
            {
                var result = await UpsertEventTicketTypeAsync(eventId, request, cancellationToken);

                if (result is ApiResultSuccess success)
                {
                    successCount++;
                    results.Add(new
                    {
                        request.Id,
                        request.Name,
                        Success = true,
                        Message = success.Message,
                        Data = success.Data
                    });
                    continue;
                }

                failureCount++;
                results.Add(new
                {
                    request.Id,
                    request.Name,
                    Success = false,
                    Message = result.Message
                });

                if (allOrNothing)
                {
                    if (transaction != null)
                        await transaction.RollbackAsync(cancellationToken);

                    return ApiResponse.ErrorWithMessage(
                        ErrorHttp.BadRequest,
                        $"Bulk upsert failed for ticket type {request.Name}: {result.Message}");
                }
            }

            if (allOrNothing && transaction != null)
                await transaction.CommitAsync(cancellationToken);

            await UpdateEventPriceFromAsync(evt, force: true, cancellationToken);

            return ApiResponse.Success(new
            {
                Total = results.Count,
                Succeeded = successCount,
                Failed = failureCount,
                Results = results
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error bulk upserting event ticket types for event {EventId}", eventId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpsertEventScheduleAsync(
        Guid eventId,
        List<EventScheduleItemUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (requests == null || requests.Count == 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Request list is empty.");

            var evt = await GetEventForUpdateAsync(eventId, userContext.TenantId, cancellationToken);
            if (evt == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            await using IDbContextTransaction? transaction = allOrNothing
                ? await eventScheduleItemRepository.BeginTransactionAsync()
                : null;

            var results = new List<object>();
            var successCount = 0;
            var failureCount = 0;

            foreach (var request in requests)
            {
                var result = await UpsertEventScheduleItemAsync(eventId, request, cancellationToken);

                if (result is ApiResultSuccess success)
                {
                    successCount++;
                    results.Add(new
                    {
                        request.Id,
                        request.Title,
                        Success = true,
                        Message = success.Message,
                        Data = success.Data
                    });
                    continue;
                }

                failureCount++;
                results.Add(new
                {
                    request.Id,
                    request.Title,
                    Success = false,
                    Message = result.Message
                });

                if (allOrNothing)
                {
                    if (transaction != null)
                        await transaction.RollbackAsync(cancellationToken);

                    return ApiResponse.ErrorWithMessage(
                        ErrorHttp.BadRequest,
                        $"Bulk upsert failed for schedule item {request.Title}: {result.Message}");
                }
            }

            if (allOrNothing && transaction != null)
                await transaction.CommitAsync(cancellationToken);

            return ApiResponse.Success(new
            {
                Total = results.Count,
                Succeeded = successCount,
                Failed = failureCount,
                Results = results
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error bulk upserting event schedule for event {EventId}", eventId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpsertEventPoliciesAsync(
        Guid eventId,
        List<EventPolicyUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (requests == null || requests.Count == 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Request list is empty.");

            var evt = await GetEventForUpdateAsync(eventId, userContext.TenantId, cancellationToken);
            if (evt == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            await using IDbContextTransaction? transaction = allOrNothing
                ? await eventPolicyRepository.BeginTransactionAsync()
                : null;

            var results = new List<object>();
            var successCount = 0;
            var failureCount = 0;

            foreach (var request in requests)
            {
                var result = await UpsertEventPolicyAsync(eventId, request, cancellationToken);

                if (result is ApiResultSuccess success)
                {
                    successCount++;
                    results.Add(new
                    {
                        request.Id,
                        request.Title,
                        Success = true,
                        Message = success.Message,
                        Data = success.Data
                    });
                    continue;
                }

                failureCount++;
                results.Add(new
                {
                    request.Id,
                    request.Title,
                    Success = false,
                    Message = result.Message
                });

                if (allOrNothing)
                {
                    if (transaction != null)
                        await transaction.RollbackAsync(cancellationToken);

                    return ApiResponse.ErrorWithMessage(
                        ErrorHttp.BadRequest,
                        $"Bulk upsert failed for policy {request.Title}: {result.Message}");
                }
            }

            if (allOrNothing && transaction != null)
                await transaction.CommitAsync(cancellationToken);

            return ApiResponse.Success(new
            {
                Total = results.Count,
                Succeeded = successCount,
                Failed = failureCount,
                Results = results
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error bulk upserting event policies for event {EventId}", eventId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpsertEventMediaAsync(
        Guid eventId,
        List<EventMediaUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (requests == null || requests.Count == 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Request list is empty.");

            var evt = await GetEventForUpdateAsync(eventId, userContext.TenantId, cancellationToken);
            if (evt == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (requests.Any(r => r.IsCover))
            {
                var existingCovers = await eventMediaRepository.Query()
                    .Where(m =>
                        m.FkEventId == eventId &&
                        m.IsCover &&
                        m.Active &&
                        !m.Deleted)
                    .ToListAsync(cancellationToken);

                foreach (var cover in existingCovers)
                {
                    cover.IsCover = false;
                    await eventMediaRepository.UpdateAsync(cover);
                }
            }

            await using IDbContextTransaction? transaction = allOrNothing
                ? await eventMediaRepository.BeginTransactionAsync()
                : null;

            var results = new List<object>();
            var successCount = 0;
            var failureCount = 0;

            foreach (var request in requests)
            {
                var result = await UpsertEventMediaItemAsync(eventId, request, cancellationToken);

                if (result is ApiResultSuccess success)
                {
                    successCount++;
                    results.Add(new
                    {
                        request.Id,
                        request.Url,
                        Success = true,
                        Message = success.Message,
                        Data = success.Data
                    });
                    continue;
                }

                failureCount++;
                results.Add(new
                {
                    request.Id,
                    request.Url,
                    Success = false,
                    Message = result.Message
                });

                if (allOrNothing)
                {
                    if (transaction != null)
                        await transaction.RollbackAsync(cancellationToken);

                    return ApiResponse.ErrorWithMessage(
                        ErrorHttp.BadRequest,
                        $"Bulk upsert failed for media {request.Url}: {result.Message}");
                }
            }

            if (allOrNothing && transaction != null)
                await transaction.CommitAsync(cancellationToken);

            await UpdateEventCoverAsync(evt, cancellationToken);

            return ApiResponse.Success(new
            {
                Total = results.Count,
                Succeeded = successCount,
                Failed = failureCount,
                Results = results
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error bulk upserting event media for event {EventId}", eventId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    private async Task<ApiResult> UpsertEventTicketTypeAsync(
        Guid eventId,
        EventTicketTypeUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidateTicketTypeRequest(request);
        if (validation != null)
            return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            var existing = await eventTicketTypeRepository.GetByIdAsync(request.Id.Value);
            if (existing == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (existing.FkEventId != eventId)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Ticket type does not belong to this event.");

            existing.Name = request.Name.Trim();
            existing.PriceAmount = request.PriceAmount;
            existing.Currency = NormalizeCurrency(request.Currency);
            existing.Capacity = request.Capacity;
            existing.MinPerOrder = request.MinPerOrder;
            existing.MaxPerOrder = request.MaxPerOrder;
            existing.SalesStartUtc = NormalizeUtc(request.SalesStartUtc);
            existing.SalesEndUtc = NormalizeUtc(request.SalesEndUtc);
            existing.Perks = EventConverter.SerializePerks(request.Perks);

            var updated = await eventTicketTypeRepository.UpdateAsync(existing);
            return ApiResponse.Success(EventConverter.ToDto(updated));
        }

        var created = new EventTicketType
        {
            FkEventId = eventId,
            Name = request.Name.Trim(),
            PriceAmount = request.PriceAmount,
            Currency = NormalizeCurrency(request.Currency),
            Capacity = request.Capacity,
            MinPerOrder = request.MinPerOrder,
            MaxPerOrder = request.MaxPerOrder,
            SalesStartUtc = NormalizeUtc(request.SalesStartUtc),
            SalesEndUtc = NormalizeUtc(request.SalesEndUtc),
            Perks = EventConverter.SerializePerks(request.Perks),
            Active = true,
            Deleted = false
        };

        var added = await eventTicketTypeRepository.AddAsync(created);
        return ApiResponse.Success(EventConverter.ToDto(added));
    }

    private async Task<ApiResult> UpsertEventScheduleItemAsync(
        Guid eventId,
        EventScheduleItemUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidateScheduleRequest(request);
        if (validation != null)
            return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            var existing = await eventScheduleItemRepository.GetByIdAsync(request.Id.Value);
            if (existing == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (existing.FkEventId != eventId)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Schedule item does not belong to this event.");

            existing.StartUtc = NormalizeUtc(request.StartUtc);
            existing.Title = request.Title.Trim();
            existing.Subtitle = request.Subtitle?.Trim();
            existing.SortOrder = request.SortOrder;

            var updated = await eventScheduleItemRepository.UpdateAsync(existing);
            return ApiResponse.Success(EventConverter.ToDto(updated));
        }

        var created = new EventScheduleItem
        {
            FkEventId = eventId,
            StartUtc = NormalizeUtc(request.StartUtc),
            Title = request.Title.Trim(),
            Subtitle = request.Subtitle?.Trim(),
            SortOrder = request.SortOrder,
            Active = true,
            Deleted = false
        };

        var added = await eventScheduleItemRepository.AddAsync(created);
        return ApiResponse.Success(EventConverter.ToDto(added));
    }

    private async Task<ApiResult> UpsertEventPolicyAsync(
        Guid eventId,
        EventPolicyUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidatePolicyRequest(request);
        if (validation != null)
            return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            var existing = await eventPolicyRepository.GetByIdAsync(request.Id.Value);
            if (existing == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (existing.FkEventId != eventId)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Policy does not belong to this event.");

            existing.Title = request.Title.Trim();
            existing.Body = request.Body?.Trim();
            existing.SortOrder = request.SortOrder;

            var updated = await eventPolicyRepository.UpdateAsync(existing);
            return ApiResponse.Success(EventConverter.ToDto(updated));
        }

        var created = new EventPolicy
        {
            FkEventId = eventId,
            Title = request.Title.Trim(),
            Body = request.Body?.Trim(),
            SortOrder = request.SortOrder,
            Active = true,
            Deleted = false
        };

        var added = await eventPolicyRepository.AddAsync(created);
        return ApiResponse.Success(EventConverter.ToDto(added));
    }

    private async Task<ApiResult> UpsertEventMediaItemAsync(
        Guid eventId,
        EventMediaUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidateMediaRequest(request);
        if (validation != null)
            return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            var existing = await eventMediaRepository.GetByIdAsync(request.Id.Value);
            if (existing == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (existing.FkEventId != eventId)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Media does not belong to this event.");

            existing.Url = request.Url.Trim();
            existing.Caption = request.Caption?.Trim();
            existing.IsCover = request.IsCover;
            existing.SortOrder = request.SortOrder;

            var updated = await eventMediaRepository.UpdateAsync(existing);
            return ApiResponse.Success(EventConverter.ToDto(updated));
        }

        var created = new EventMedia
        {
            FkEventId = eventId,
            Url = request.Url.Trim(),
            Caption = request.Caption?.Trim(),
            IsCover = request.IsCover,
            SortOrder = request.SortOrder,
            Active = true,
            Deleted = false
        };

        var added = await eventMediaRepository.AddAsync(created);
        return ApiResponse.Success(EventConverter.ToDto(added));
    }

    private async Task CreateTicketTypesAsync(
        Guid eventId,
        List<EventTicketTypeUpsertRequest> requests,
        CancellationToken cancellationToken)
    {
        foreach (var request in requests)
        {
            var validation = ValidateTicketTypeRequest(request);
            if (validation != null)
                throw new InvalidOperationException(validation);

            var created = new EventTicketType
            {
                FkEventId = eventId,
                Name = request.Name.Trim(),
                PriceAmount = request.PriceAmount,
                Currency = NormalizeCurrency(request.Currency),
                Capacity = request.Capacity,
                MinPerOrder = request.MinPerOrder,
                MaxPerOrder = request.MaxPerOrder,
                SalesStartUtc = NormalizeUtc(request.SalesStartUtc),
                SalesEndUtc = NormalizeUtc(request.SalesEndUtc),
                Perks = EventConverter.SerializePerks(request.Perks),
                Active = true,
                Deleted = false
            };

            await eventTicketTypeRepository.AddAsync(created);
        }
    }

    private async Task CreateScheduleItemsAsync(
        Guid eventId,
        List<EventScheduleItemUpsertRequest> requests,
        CancellationToken cancellationToken)
    {
        foreach (var request in requests)
        {
            var validation = ValidateScheduleRequest(request);
            if (validation != null)
                throw new InvalidOperationException(validation);

            var created = new EventScheduleItem
            {
                FkEventId = eventId,
                StartUtc = NormalizeUtc(request.StartUtc),
                Title = request.Title.Trim(),
                Subtitle = request.Subtitle?.Trim(),
                SortOrder = request.SortOrder,
                Active = true,
                Deleted = false
            };

            await eventScheduleItemRepository.AddAsync(created);
        }
    }

    private async Task CreatePoliciesAsync(
        Guid eventId,
        List<EventPolicyUpsertRequest> requests,
        CancellationToken cancellationToken)
    {
        foreach (var request in requests)
        {
            var validation = ValidatePolicyRequest(request);
            if (validation != null)
                throw new InvalidOperationException(validation);

            var created = new EventPolicy
            {
                FkEventId = eventId,
                Title = request.Title.Trim(),
                Body = request.Body?.Trim(),
                SortOrder = request.SortOrder,
                Active = true,
                Deleted = false
            };

            await eventPolicyRepository.AddAsync(created);
        }
    }

    private async Task CreateMediaAsync(
        Event evt,
        List<EventMediaUpsertRequest> requests,
        CancellationToken cancellationToken)
    {
        foreach (var request in requests)
        {
            var validation = ValidateMediaRequest(request);
            if (validation != null)
                throw new InvalidOperationException(validation);

            var created = new EventMedia
            {
                FkEventId = evt.Id,
                Url = request.Url.Trim(),
                Caption = request.Caption?.Trim(),
                IsCover = request.IsCover,
                SortOrder = request.SortOrder,
                Active = true,
                Deleted = false
            };

            await eventMediaRepository.AddAsync(created);
        }
    }

    private async Task<Event?> GetEventWithIncludesAsync(
        Guid eventId,
        Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var query = eventRepository.Query()
            .AsNoTracking()
            .Include(e => e.FkTenant)
            .Include(e => e.FkEventCategory)
            .Include(e => e.FkVenue)
            .Include(e => e.FkCoverMedia)
            .Where(e =>
                e.Id == eventId &&
                e.Active &&
                !e.Deleted);

        if (tenantId.HasValue)
            query = query.Where(e => e.FkTenantId == tenantId.Value);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<Event?> GetEventForUpdateAsync(
        Guid eventId,
        Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var query = eventRepository.Query()
            .Where(e =>
                e.Id == eventId &&
                e.Active &&
                !e.Deleted);

        if (tenantId.HasValue)
            query = query.Where(e => e.FkTenantId == tenantId.Value);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<bool> TenantSupportsEventsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var code = BookingCategoryTypeMapper.ToCode(BookingCategoryType.Events);
        return await tenantBookingCategoryRepository.Query()
            .AsNoTracking()
            .AnyAsync(tbc =>
                tbc.FkTenantId == tenantId &&
                tbc.IsEnabled &&
                tbc.Active &&
                !tbc.Deleted &&
                tbc.FkBookingCategory.Code == code &&
                tbc.FkBookingCategory.Active &&
                !tbc.FkBookingCategory.Deleted,
                cancellationToken);
    }

    private async Task UpdateEventPriceFromAsync(Event evt, bool force, CancellationToken cancellationToken)
    {
        if (!force && evt.PriceFrom.HasValue)
            return;

        var minPrice = await eventTicketTypeRepository.Query()
            .AsNoTracking()
            .Where(t =>
                t.FkEventId == evt.Id &&
                t.Active &&
                !t.Deleted)
            .MinAsync(t => (decimal?)t.PriceAmount, cancellationToken);

        if (minPrice.HasValue)
        {
            evt.PriceFrom = minPrice.Value;
            await eventRepository.UpdateAsync(evt);
        }
    }

    private async Task UpdateEventCoverAsync(Event evt, CancellationToken cancellationToken)
    {
        var cover = await eventMediaRepository.Query()
            .AsNoTracking()
            .Where(m =>
                m.FkEventId == evt.Id &&
                m.Active &&
                !m.Deleted &&
                m.IsCover)
            .OrderBy(m => m.SortOrder)
            .FirstOrDefaultAsync(cancellationToken);

        evt.FkCoverMediaId = cover?.Id;
        await eventRepository.UpdateAsync(evt);
    }

    private static IQueryable<Event> ApplySorting(IQueryable<Event> query, string? sortBy, string? sortDirection)
    {
        var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "title" => isDesc ? query.OrderByDescending(e => e.Title) : query.OrderBy(e => e.Title),
            "endutc" => isDesc ? query.OrderByDescending(e => e.EndUtc) : query.OrderBy(e => e.EndUtc),
            "status" => isDesc ? query.OrderByDescending(e => e.Status) : query.OrderBy(e => e.Status),
            "price" => isDesc ? query.OrderByDescending(e => e.PriceFrom) : query.OrderBy(e => e.PriceFrom),
            _ => isDesc ? query.OrderByDescending(e => e.StartUtc) : query.OrderBy(e => e.StartUtc)
        };
    }

    private async Task<Dictionary<Guid, PriceInfo>> BuildPriceLookupAsync(
        List<Guid> eventIds,
        CancellationToken cancellationToken)
    {
        if (eventIds.Count == 0)
            return new Dictionary<Guid, PriceInfo>();

        var tickets = await eventTicketTypeRepository.Query()
            .AsNoTracking()
            .Where(t =>
                eventIds.Contains(t.FkEventId) &&
                t.Active &&
                !t.Deleted)
            .Select(t => new { t.FkEventId, t.PriceAmount, t.Currency })
            .ToListAsync(cancellationToken);

        return tickets
            .GroupBy(t => t.FkEventId)
            .ToDictionary(
                g => g.Key,
                g => new PriceInfo(
                    g.Min(x => x.PriceAmount),
                    g.OrderBy(x => x.PriceAmount).Select(x => x.Currency).FirstOrDefault()));
    }

    private static PriceInfo ResolvePrice(Event evt, Dictionary<Guid, PriceInfo> lookup)
    {
        if (!lookup.TryGetValue(evt.Id, out var info))
            info = new PriceInfo(null, null);

        var price = evt.PriceFrom ?? info.PriceFrom;
        return new PriceInfo(price, info.Currency);
    }

    private static PriceInfo ResolvePrice(Event evt, IReadOnlyCollection<EventTicketType> tickets)
    {
        if (tickets.Count == 0)
            return new PriceInfo(evt.PriceFrom, null);

        var cheapest = tickets.OrderBy(t => t.PriceAmount).First();
        var price = evt.PriceFrom ?? cheapest.PriceAmount;
        return new PriceInfo(price, cheapest.Currency);
    }

    private static string? ValidateEventRequest(string? title, DateTime startUtc, DateTime endUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "Title is required.";

        if (endUtc <= startUtc)
            return "EndUtc must be greater than StartUtc.";

        return null;
    }

    private static string? ValidateTicketTypeRequest(EventTicketTypeUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return "Ticket type name is required.";

        if (request.PriceAmount < 0)
            return "PriceAmount must be zero or greater.";

        if (request.MinPerOrder.HasValue && request.MinPerOrder.Value < 0)
            return "MinPerOrder cannot be negative.";

        if (request.MaxPerOrder.HasValue && request.MaxPerOrder.Value < 0)
            return "MaxPerOrder cannot be negative.";

        if (request.MinPerOrder.HasValue && request.MaxPerOrder.HasValue &&
            request.MinPerOrder.Value > request.MaxPerOrder.Value)
            return "MinPerOrder must be less than or equal to MaxPerOrder.";

        var start = request.SalesStartUtc;
        var end = request.SalesEndUtc;
        if (start.HasValue != end.HasValue)
            return "SalesStartUtc and SalesEndUtc must be provided together.";

        if (start.HasValue && end.HasValue && end.Value <= start.Value)
            return "SalesEndUtc must be greater than SalesStartUtc.";

        return null;
    }

    private static string? ValidateScheduleRequest(EventScheduleItemUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return "Schedule title is required.";

        if (request.StartUtc == default)
            return "StartUtc is required.";

        return null;
    }

    private static string? ValidatePolicyRequest(EventPolicyUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return "Policy title is required.";

        return null;
    }

    private static string? ValidateMediaRequest(EventMediaUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Url))
            return "Media URL is required.";

        return null;
    }

    private bool IsTenantMismatch(Guid tenantId)
    {
        return userContext.TenantId.HasValue && userContext.TenantId.Value != tenantId;
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    private static DateTime? NormalizeUtc(DateTime? value)
    {
        return value.HasValue ? NormalizeUtc(value.Value) : null;
    }

    private static string NormalizeCurrency(string? currency)
    {
        return string.IsNullOrWhiteSpace(currency)
            ? "USD"
            : currency.Trim().ToUpperInvariant();
    }

    private async Task<List<VwEventTicketType>> BuildTicketTypeDtosAsync(
        Guid eventId,
        List<EventTicketType> ticketTypes,
        CancellationToken cancellationToken)
    {
        if (ticketTypes.Count == 0)
            return new List<VwEventTicketType>();

        var nowUtc = DateTime.UtcNow;
        var ticketTypeIds = ticketTypes.Select(t => t.Id).ToList();
        var reservedLookup = await eventOrderItemRepository.GetReservedQuantitiesAsync(
            eventId,
            ticketTypeIds,
            nowUtc,
            cancellationToken);

        return ticketTypes
            .Select(t => EventConverter.ToDto(t, ResolveAvailableQuantity(t, reservedLookup)))
            .ToList();
    }

    private static int? ResolveAvailableQuantity(
        EventTicketType ticketType,
        Dictionary<Guid, int> reservedLookup)
    {
        if (!ticketType.Capacity.HasValue)
            return null;

        reservedLookup.TryGetValue(ticketType.Id, out var reserved);
        var available = ticketType.Capacity.Value - reserved;
        return available < 0 ? 0 : available;
    }

    private sealed record PriceInfo(decimal? PriceFrom, string? Currency);
}
