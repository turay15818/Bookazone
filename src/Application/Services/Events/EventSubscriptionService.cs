using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Converter;
using Bookazone.Application.DTOs.Events;
using Bookazone.Application.Interfaces.Context;
using Bookazone.Application.Interfaces.Repository.Events;
using Bookazone.Application.Interfaces.Services.Events;
using Bookazone.Domain.Entities.Events;

namespace Bookazone.Application.Services.Events;

public class EventSubscriptionService(
    IEventCategorySubscriptionRepository eventCategorySubscriptionRepository,
    IEventCategoryRepository eventCategoryRepository,
    IUserContext userContext,
    ILogger<EventSubscriptionService> logger)
    : IEventSubscriptionService
{
    public async Task<ApiResult> GetMySubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!userContext.UserId.HasValue)
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User context does not contain a valid user id."));

            var userId = userContext.UserId.Value;
            var subscriptions = await eventCategorySubscriptionRepository.Query()
                .AsNoTracking()
                .Include(s => s.FkEventCategory)
                .Where(s =>
                    s.FkUserId == userId &&
                    s.Active &&
                    !s.Deleted)
                .OrderBy(s => s.FkEventCategory.Name)
                .ToListAsync(cancellationToken);

            var dto = subscriptions.Select(EventConverter.ToDto);
            return ApiResponse.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting event subscriptions");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpsertSubscriptionAsync(
        EventCategorySubscriptionUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!userContext.UserId.HasValue)
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User context does not contain a valid user id."));

            if (request.EventCategoryId == Guid.Empty)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "EventCategoryId is required.");

            var category = await eventCategoryRepository.GetByIdAsync(request.EventCategoryId);
            if (category == null || !category.Active || category.Deleted)
                return ApiResponse.ErrorWithMessage(ErrorHttp.NotFound, "Event category not found or inactive.");

            var userId = userContext.UserId.Value;
            var existing = await eventCategorySubscriptionRepository.GetByUserAndCategoryAsync(
                userId,
                request.EventCategoryId,
                cancellationToken);

            if (!request.Subscribe)
            {
                if (existing == null)
                    return ApiResponse.Success(null);

                existing.ReceivePush = request.ReceivePush;
                existing.ReceiveEmail = request.ReceiveEmail;
                existing.Active = false;
                existing.Deleted = true;
                var updated = await eventCategorySubscriptionRepository.UpdateAsync(existing);
                updated.FkEventCategory = category;
                return ApiResponse.Success(EventConverter.ToDto(updated));
            }

            if (existing != null)
            {
                existing.ReceivePush = request.ReceivePush;
                existing.ReceiveEmail = request.ReceiveEmail;
                existing.Active = true;
                existing.Deleted = false;
                var updated = await eventCategorySubscriptionRepository.UpdateAsync(existing);
                updated.FkEventCategory = category;
                return ApiResponse.Success(EventConverter.ToDto(updated));
            }

            var subscription = new EventCategorySubscription
            {
                FkUserId = userId,
                FkEventCategoryId = request.EventCategoryId,
                ReceivePush = request.ReceivePush,
                ReceiveEmail = request.ReceiveEmail,
                Active = true,
                Deleted = false
            };

            var created = await eventCategorySubscriptionRepository.AddAsync(subscription);
            created.FkEventCategory = category;
            return ApiResponse.Success(EventConverter.ToDto(created));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting event subscription for category {CategoryId}", request.EventCategoryId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpsertSubscriptionsAsync(
        List<EventCategorySubscriptionUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (requests == null || requests.Count == 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Request list is empty.");

            await using IDbContextTransaction? transaction = allOrNothing
                ? await eventCategorySubscriptionRepository.BeginTransactionAsync()
                : null;

            var results = new List<object>();
            var successCount = 0;
            var failureCount = 0;

            foreach (var request in requests)
            {
                var result = await UpsertSubscriptionAsync(request, cancellationToken);

                if (result is ApiResultSuccess success)
                {
                    successCount++;
                    results.Add(new
                    {
                        request.EventCategoryId,
                        request.Subscribe,
                        Success = true,
                        Message = success.Message,
                        Data = success.Data
                    });
                    continue;
                }

                failureCount++;
                results.Add(new
                {
                    request.EventCategoryId,
                    request.Subscribe,
                    Success = false,
                    Message = result.Message
                });

                if (allOrNothing)
                {
                    if (transaction != null)
                        await transaction.RollbackAsync(cancellationToken);

                    return ApiResponse.ErrorWithMessage(
                        ErrorHttp.BadRequest,
                        $"Bulk upsert failed for category {request.EventCategoryId}: {result.Message}");
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
            logger.LogError(ex, "Error bulk upserting event subscriptions");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
