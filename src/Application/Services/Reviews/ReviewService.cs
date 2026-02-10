using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Response;
using Bookazone.Application.DTOs.Reviews;
using Bookazone.Application.Interfaces.Context;
using Bookazone.Application.Interfaces.Repository.Booking;
using Bookazone.Application.Interfaces.Repository.Events;
using Bookazone.Application.Interfaces.Repository.Reviews;
using Bookazone.Application.Interfaces.Repository.Vehicles;
using Bookazone.Application.Interfaces.Services.Reviews;
using Bookazone.Domain.Entities.Events;
using Bookazone.Domain.Entities.Reviews;
using Bookazone.Domain.Entities.Vehicles;
using Bookazone.Domain.Enums;
using BookingEntity = Bookazone.Domain.Entities.Booking.Booking;

namespace Bookazone.Application.Services.Reviews;

public class ReviewService(
    IReviewRepository reviewRepository,
    IReviewAspectRatingRepository reviewAspectRatingRepository,
    IBookingRepository bookingRepository,
    IEventOrderRepository eventOrderRepository,
    IVehicleOrderRepository vehicleOrderRepository,
    IUserContext userContext,
    IHttpContextAccessor httpContextAccessor,
    ILogger<ReviewService> logger)
    : IReviewService
{
    private const int MaxAspectCount = 10;

    public async Task<ApiResult> CreateAsync(ReviewCreateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!userContext.UserId.HasValue)
                return ApiResponse.Error(ErrorHttp.Forbidden,
                    new Exception("User context does not contain a valid user id."));

            if (request.TargetId == Guid.Empty)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "TargetId is required.");

            if (request.SourceId == Guid.Empty)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "SourceId is required.");

            if (request.Rating is < 1 or > 5)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Rating must be between 1 and 5.");

            var reviewerId = userContext.UserId.Value;
            var existing = await reviewRepository.Query()
                .AsNoTracking()
                .AnyAsync(r =>
                        r.SourceType == request.SourceType &&
                        r.SourceId == request.SourceId &&
                        r.Active &&
                        !r.Deleted,
                    cancellationToken);

            if (existing)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Review already exists for this order.");

            var sourceResult = await ValidateSourceAsync(request, reviewerId, cancellationToken);
            if (!sourceResult.IsValid)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, sourceResult.ErrorMessage ?? "Invalid review source.");

            if (!TryNormalizeAspects(request.Aspects, out var aspects, out var aspectError))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, aspectError ?? "Invalid review aspects.");

            var review = new Review
            {
                FkTenantId = sourceResult.TenantId,
                FkCustomerId = reviewerId,
                TargetType = request.TargetType,
                TargetId = request.TargetId,
                SourceType = request.SourceType,
                SourceId = request.SourceId,
                Rating = request.Rating,
                Title = NormalizeText(request.Title, 150),
                Body = NormalizeText(request.Body, 2000),
                IsVerified = sourceResult.IsVerified,
                Status = ReviewStatus.Pending,
                Active = true,
                Deleted = false
            };

            await using var transaction = await reviewRepository.BeginTransactionAsync();
            var created = await reviewRepository.AddAsync(review);

            if (aspects.Count > 0)
            {
                foreach (var aspect in aspects)
                {
                    aspect.FkReviewId = created.Id;
                    await reviewAspectRatingRepository.AddAsync(aspect);
                }
            }

            await transaction.CommitAsync(cancellationToken);

            var detail = await LoadReviewDetailAsync(created.Id, cancellationToken);
            return detail == null
                ? ApiResponse.Success(created.Id)
                : ApiResponse.Success(detail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating review");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetReviewAsync(Guid reviewId, CancellationToken cancellationToken = default)
    {
        try
        {
            var review = await LoadReviewAsync(reviewId, cancellationToken);
            if (review == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!IsTenantActor(review.FkTenantId) && !IsCustomerActor(review))
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User is not authorized to view this review."));

            return ApiResponse.Success(ToDetail(review));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting review {ReviewId}", reviewId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetPublicReviewsAsync(
        ReviewTargetType targetType,
        Guid targetId,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (targetId == Guid.Empty)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "TargetId is required.");

            var query = reviewRepository.Query()
                .AsNoTracking()
                .Where(r =>
                    r.TargetType == targetType &&
                    r.TargetId == targetId &&
                    r.Status == ReviewStatus.Approved &&
                    r.Active &&
                    !r.Deleted);

            var projected = query.Select(r => new VwReviewListItem
            {
                Id = r.Id,
                TenantId = r.FkTenantId,
                TenantName = r.FkTenant.Name,
                TargetType = r.TargetType,
                TargetId = r.TargetId,
                SourceType = r.SourceType,
                SourceId = r.SourceId,
                CustomerId = r.FkCustomerId,
                CustomerName = ((r.FkCustomer.Firstname ?? "") + " " + (r.FkCustomer.Lastname ?? "")).Trim(),
                CustomerAvatarUrl = r.FkCustomer.ProfileImage,
                Rating = r.Rating,
                Title = r.Title,
                Body = r.Body,
                IsVerified = r.IsVerified,
                Status = r.Status,
                Reply = r.Reply,
                ReplyAtUtc = r.RepliedAtUtc,
                CreatedAtUtc = r.DateCreated
            });

            var sorted = ApplySorting(projected, sortBy, sortDirection);
            var paged = await Paginate<VwReviewListItem>.CreateAsync(sorted, pageIndex, pageSize);

            var request = httpContextAccessor.HttpContext?.Request;
            foreach (var item in paged.Data)
                item.CustomerAvatarUrl = FileUrlHelper.BuildPublicUrl(request, item.CustomerAvatarUrl);

            return ApiResponse.Success(paged);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting public reviews for {TargetType} {TargetId}", targetType, targetId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetCustomerReviewsAsync(
        List<ReviewStatus>? statuses = null,
        ReviewTargetType? targetType = null,
        Guid? targetId = null,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!userContext.UserId.HasValue)
                return ApiResponse.Error(ErrorHttp.Forbidden,
                    new Exception("User context does not contain a valid user id."));

            var customerId = userContext.UserId.Value;
            var query = reviewRepository.Query()
                .AsNoTracking()
                .Where(r =>
                    r.FkCustomerId == customerId &&
                    r.Active &&
                    !r.Deleted);

            if (targetType.HasValue)
                query = query.Where(r => r.TargetType == targetType.Value);

            if (targetId.HasValue)
                query = query.Where(r => r.TargetId == targetId.Value);

            if (statuses is { Count: > 0 })
                query = query.Where(r => statuses.Contains(r.Status));

            var projected = query.Select(r => new VwReviewListItem
            {
                Id = r.Id,
                TenantId = r.FkTenantId,
                TenantName = r.FkTenant.Name,
                TargetType = r.TargetType,
                TargetId = r.TargetId,
                SourceType = r.SourceType,
                SourceId = r.SourceId,
                CustomerId = r.FkCustomerId,
                CustomerName = ((r.FkCustomer.Firstname ?? "") + " " + (r.FkCustomer.Lastname ?? "")).Trim(),
                CustomerAvatarUrl = r.FkCustomer.ProfileImage,
                Rating = r.Rating,
                Title = r.Title,
                Body = r.Body,
                IsVerified = r.IsVerified,
                Status = r.Status,
                Reply = r.Reply,
                ReplyAtUtc = r.RepliedAtUtc,
                CreatedAtUtc = r.DateCreated
            });

            var sorted = ApplySorting(projected, sortBy, sortDirection);
            var paged = await Paginate<VwReviewListItem>.CreateAsync(sorted, pageIndex, pageSize);

            var request = httpContextAccessor.HttpContext?.Request;
            foreach (var item in paged.Data)
                item.CustomerAvatarUrl = FileUrlHelper.BuildPublicUrl(request, item.CustomerAvatarUrl);

            return ApiResponse.Success(paged);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting customer reviews");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetTenantReviewsAsync(
        List<ReviewStatus>? statuses = null,
        ReviewTargetType? targetType = null,
        Guid? targetId = null,
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
            var query = reviewRepository.Query()
                .AsNoTracking()
                .Where(r =>
                    r.FkTenantId == tenantId &&
                    r.Active &&
                    !r.Deleted);

            if (targetType.HasValue)
                query = query.Where(r => r.TargetType == targetType.Value);

            if (targetId.HasValue)
                query = query.Where(r => r.TargetId == targetId.Value);

            if (statuses is { Count: > 0 })
                query = query.Where(r => statuses.Contains(r.Status));

            var projected = query.Select(r => new VwReviewListItem
            {
                Id = r.Id,
                TenantId = r.FkTenantId,
                TenantName = r.FkTenant.Name,
                TargetType = r.TargetType,
                TargetId = r.TargetId,
                SourceType = r.SourceType,
                SourceId = r.SourceId,
                CustomerId = r.FkCustomerId,
                CustomerName = ((r.FkCustomer.Firstname ?? "") + " " + (r.FkCustomer.Lastname ?? "")).Trim(),
                CustomerAvatarUrl = r.FkCustomer.ProfileImage,
                Rating = r.Rating,
                Title = r.Title,
                Body = r.Body,
                IsVerified = r.IsVerified,
                Status = r.Status,
                Reply = r.Reply,
                ReplyAtUtc = r.RepliedAtUtc,
                CreatedAtUtc = r.DateCreated
            });

            var sorted = ApplySorting(projected, sortBy, sortDirection);
            var paged = await Paginate<VwReviewListItem>.CreateAsync(sorted, pageIndex, pageSize);

            var request = httpContextAccessor.HttpContext?.Request;
            foreach (var item in paged.Data)
                item.CustomerAvatarUrl = FileUrlHelper.BuildPublicUrl(request, item.CustomerAvatarUrl);

            return ApiResponse.Success(paged);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant reviews");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetSummaryAsync(ReviewTargetType targetType, Guid targetId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (targetId == Guid.Empty)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "TargetId is required.");

            var query = reviewRepository.Query()
                .AsNoTracking()
                .Where(r =>
                    r.TargetType == targetType &&
                    r.TargetId == targetId &&
                    r.Status == ReviewStatus.Approved &&
                    r.Active &&
                    !r.Deleted);

            var total = await query.CountAsync(cancellationToken);
            decimal average = 0m;
            if (total > 0)
            {
                var avg = await query.AverageAsync(r => r.Rating, cancellationToken);
                average = Math.Round((decimal)avg, 2, MidpointRounding.AwayFromZero);
            }

            var breakdown = await query
                .GroupBy(r => r.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var ratings = new List<VwReviewRatingCount>();
            for (var rating = 1; rating <= 5; rating++)
            {
                var count = breakdown.FirstOrDefault(b => b.Rating == rating)?.Count ?? 0;
                ratings.Add(new VwReviewRatingCount { Rating = rating, Count = count });
            }

            var summary = new VwReviewSummary
            {
                TargetType = targetType,
                TargetId = targetId,
                AverageRating = average,
                TotalReviews = total,
                Ratings = ratings
            };

            return ApiResponse.Success(summary);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting review summary for {TargetType} {TargetId}", targetType, targetId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpdateStatusAsync(ReviewStatusUpdateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var review = await GetReviewForUpdateAsync(request.ReviewId, cancellationToken);
            if (review == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!IsTenantActor(review.FkTenantId))
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User is not authorized for this tenant."));

            if (request.Status == ReviewStatus.Pending)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Review status cannot be reset to pending.");

            review.Status = request.Status;
            review.ModerationNotes = NormalizeText(request.ModerationNotes, 500);
            await reviewRepository.UpdateAsync(review);

            var detail = await LoadReviewDetailAsync(review.Id, cancellationToken);
            return detail == null
                ? ApiResponse.Success(review.Id)
                : ApiResponse.Success(detail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating review status {ReviewId}", request.ReviewId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> ReplyAsync(ReviewReplyRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Reply))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Reply is required.");

            var review = await GetReviewForUpdateAsync(request.ReviewId, cancellationToken);
            if (review == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!IsTenantActor(review.FkTenantId))
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User is not authorized for this tenant."));

            if (review.Status != ReviewStatus.Approved)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Reply is allowed only for approved reviews.");

            review.Reply = NormalizeText(request.Reply, 2000);
            review.RepliedAtUtc = DateTime.UtcNow;
            review.ReplyByUserId = userContext.UserId;

            await reviewRepository.UpdateAsync(review);

            var detail = await LoadReviewDetailAsync(review.Id, cancellationToken);
            return detail == null
                ? ApiResponse.Success(review.Id)
                : ApiResponse.Success(detail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error replying to review {ReviewId}", request.ReviewId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    private async Task<Review?> LoadReviewAsync(Guid reviewId, CancellationToken cancellationToken)
    {
        return await reviewRepository.Query()
            .AsNoTracking()
            .Include(r => r.FkTenant)
            .Include(r => r.FkCustomer)
            .Include(r => r.AspectRatings)
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.Active && !r.Deleted, cancellationToken);
    }

    private Task<Review?> GetReviewForUpdateAsync(Guid reviewId, CancellationToken cancellationToken)
    {
        return reviewRepository.Query()
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(r => r.Id == reviewId && !r.Deleted, cancellationToken);
    }

    private async Task<VwReviewDetail?> LoadReviewDetailAsync(Guid reviewId, CancellationToken cancellationToken)
    {
        var review = await LoadReviewAsync(reviewId, cancellationToken);
        return review == null ? null : ToDetail(review);
    }

    private VwReviewDetail ToDetail(Review review)
    {
        var request = httpContextAccessor.HttpContext?.Request;
        var listItem = new VwReviewListItem
        {
            Id = review.Id,
            TenantId = review.FkTenantId,
            TenantName = review.FkTenant?.Name ?? string.Empty,
            TargetType = review.TargetType,
            TargetId = review.TargetId,
            SourceType = review.SourceType,
            SourceId = review.SourceId,
            CustomerId = review.FkCustomerId,
            CustomerName = GetCustomerName(review.FkCustomer),
            CustomerAvatarUrl = FileUrlHelper.BuildPublicUrl(request, review.FkCustomer?.ProfileImage),
            Rating = review.Rating,
            Title = review.Title,
            Body = review.Body,
            IsVerified = review.IsVerified,
            Status = review.Status,
            Reply = review.Reply,
            ReplyAtUtc = review.RepliedAtUtc,
            CreatedAtUtc = review.DateCreated
        };

        return new VwReviewDetail
        {
            Id = listItem.Id,
            TenantId = listItem.TenantId,
            TenantName = listItem.TenantName,
            TargetType = listItem.TargetType,
            TargetId = listItem.TargetId,
            SourceType = listItem.SourceType,
            SourceId = listItem.SourceId,
            CustomerId = listItem.CustomerId,
            CustomerName = listItem.CustomerName,
            CustomerAvatarUrl = listItem.CustomerAvatarUrl,
            Rating = listItem.Rating,
            Title = listItem.Title,
            Body = listItem.Body,
            IsVerified = listItem.IsVerified,
            Status = listItem.Status,
            Reply = listItem.Reply,
            ReplyAtUtc = listItem.ReplyAtUtc,
            CreatedAtUtc = listItem.CreatedAtUtc,
            ModerationNotes = review.ModerationNotes,
            Aspects = review.AspectRatings
                .OrderBy(a => a.SortOrder)
                .Select(a => new VwReviewAspectRating
                {
                    Label = a.Label,
                    Score = a.Score,
                    SortOrder = a.SortOrder
                })
                .ToList()
        };
    }

    private async Task<SourceValidationResult> ValidateSourceAsync(
        ReviewCreateRequest request,
        Guid reviewerId,
        CancellationToken cancellationToken)
    {
        switch (request.SourceType)
        {
            case ReviewSourceType.Booking:
                return await ValidateBookingSourceAsync(request, reviewerId, cancellationToken);
            case ReviewSourceType.EventOrder:
                return await ValidateEventOrderSourceAsync(request, reviewerId, cancellationToken);
            case ReviewSourceType.VehicleOrder:
                return await ValidateVehicleOrderSourceAsync(request, reviewerId, cancellationToken);
            default:
                return SourceValidationResult.Fail("Unsupported review source.");
        }
    }

    private async Task<SourceValidationResult> ValidateBookingSourceAsync(
        ReviewCreateRequest request,
        Guid reviewerId,
        CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.SourceId && b.Active && !b.Deleted, cancellationToken);

        if (booking == null)
            return SourceValidationResult.Fail("Booking not found.");

        if (booking.FkCustomerId != reviewerId)
            return SourceValidationResult.Fail("Booking does not belong to the current user.");

        if (!IsBookingReviewable(booking))
            return SourceValidationResult.Fail("Booking is not completed yet.");

        if (!IsBookingTargetMatch(request, booking))
            return SourceValidationResult.Fail("Target does not match booking.");

        return SourceValidationResult.Success(booking.FkTenantId, IsBookingVerified(booking));
    }

    private async Task<SourceValidationResult> ValidateEventOrderSourceAsync(
        ReviewCreateRequest request,
        Guid reviewerId,
        CancellationToken cancellationToken)
    {
        var order = await eventOrderRepository.Query()
            .AsNoTracking()
            .Include(o => o.FkEvent)
            .FirstOrDefaultAsync(o => o.Id == request.SourceId && o.Active && !o.Deleted, cancellationToken);

        if (order == null)
            return SourceValidationResult.Fail("Event order not found.");

        if (order.FkCustomerId != reviewerId)
            return SourceValidationResult.Fail("Event order does not belong to the current user.");

        if (!IsEventOrderReviewable(order))
            return SourceValidationResult.Fail("Event order is not completed yet.");

        if (!IsEventOrderTargetMatch(request, order))
            return SourceValidationResult.Fail("Target does not match event order.");

        return SourceValidationResult.Success(order.FkTenantId, IsEventOrderVerified(order));
    }

    private async Task<SourceValidationResult> ValidateVehicleOrderSourceAsync(
        ReviewCreateRequest request,
        Guid reviewerId,
        CancellationToken cancellationToken)
    {
        var order = await vehicleOrderRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.SourceId && o.Active && !o.Deleted, cancellationToken);

        if (order == null)
            return SourceValidationResult.Fail("Vehicle order not found.");

        if (order.FkCustomerId != reviewerId)
            return SourceValidationResult.Fail("Vehicle order does not belong to the current user.");

        if (!IsVehicleOrderReviewable(order))
            return SourceValidationResult.Fail("Vehicle order is not completed yet.");

        if (!IsVehicleOrderTargetMatch(request, order))
            return SourceValidationResult.Fail("Target does not match vehicle order.");

        return SourceValidationResult.Success(order.FkTenantId, IsVehicleOrderVerified(order));
    }

    private static bool IsBookingTargetMatch(ReviewCreateRequest request, BookingEntity booking)
    {
        return request.TargetType switch
        {
            ReviewTargetType.Tenant => request.TargetId == booking.FkTenantId,
            ReviewTargetType.SportResource => request.TargetId == booking.FkSportResourceId,
            _ => false
        };
    }

    private static bool IsEventOrderTargetMatch(ReviewCreateRequest request, EventOrder order)
    {
        return request.TargetType switch
        {
            ReviewTargetType.Tenant => request.TargetId == order.FkTenantId,
            ReviewTargetType.Event => request.TargetId == order.FkEventId,
            _ => false
        };
    }

    private static bool IsVehicleOrderTargetMatch(ReviewCreateRequest request, VehicleOrder order)
    {
        return request.TargetType switch
        {
            ReviewTargetType.Tenant => request.TargetId == order.FkTenantId,
            ReviewTargetType.Vehicle => request.TargetId == order.FkVehicleId,
            _ => false
        };
    }

    private static bool IsBookingReviewable(BookingEntity booking)
    {
        if (booking.Status == BookingStatus.Completed)
            return true;

        return booking.Status == BookingStatus.Confirmed && booking.EndUtc <= DateTime.UtcNow;
    }

    private static bool IsBookingVerified(BookingEntity booking)
        => booking.Status == BookingStatus.Completed || booking.EndUtc <= DateTime.UtcNow;

    private static bool IsEventOrderReviewable(EventOrder order)
    {
        if (order.Status == EventOrderStatus.Completed)
            return true;

        return order.Status == EventOrderStatus.Confirmed &&
               order.FkEvent != null &&
               order.FkEvent.EndUtc <= DateTime.UtcNow;
    }

    private static bool IsEventOrderVerified(EventOrder order)
        => order.Status == EventOrderStatus.Completed ||
           (order.FkEvent != null && order.FkEvent.EndUtc <= DateTime.UtcNow);

    private static bool IsVehicleOrderReviewable(VehicleOrder order)
    {
        if (order.Status == VehicleOrderStatus.Completed)
            return true;

        return order.Status == VehicleOrderStatus.Confirmed &&
               order.EndUtc.HasValue &&
               order.EndUtc.Value <= DateTime.UtcNow;
    }

    private static bool IsVehicleOrderVerified(VehicleOrder order)
        => order.Status == VehicleOrderStatus.Completed ||
           (order.EndUtc.HasValue && order.EndUtc.Value <= DateTime.UtcNow);

    private static IQueryable<VwReviewListItem> ApplySorting(
        IQueryable<VwReviewListItem> query,
        string? sortBy,
        string? sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var key = sortBy?.Trim().ToLowerInvariant();

        return key switch
        {
            "rating" => descending ? query.OrderByDescending(x => x.Rating) : query.OrderBy(x => x.Rating),
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            "created" or "createdutc" => descending
                ? query.OrderByDescending(x => x.CreatedAtUtc)
                : query.OrderBy(x => x.CreatedAtUtc),
            _ => query.OrderByDescending(x => x.CreatedAtUtc)
        };
    }

    private static string? NormalizeText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private static bool TryNormalizeAspects(
        List<ReviewAspectRequest>? requests,
        out List<ReviewAspectRating> aspects,
        out string? error)
    {
        aspects = new List<ReviewAspectRating>();
        error = null;

        if (requests == null || requests.Count == 0)
            return true;

        if (requests.Count > MaxAspectCount)
        {
            error = $"Only {MaxAspectCount} aspect ratings are allowed.";
            return false;
        }

        var index = 0;
        foreach (var request in requests)
        {
            if (request == null)
                continue;

            var label = NormalizeText(request.Label, 100);
            if (string.IsNullOrWhiteSpace(label))
            {
                error = "Aspect label is required.";
                return false;
            }

            if (request.Score is < 1 or > 5)
            {
                error = "Aspect score must be between 1 and 5.";
                return false;
            }

            aspects.Add(new ReviewAspectRating
            {
                Label = label,
                Score = request.Score,
                SortOrder = request.SortOrder ?? index
            });

            index++;
        }

        return true;
    }

    private static string GetCustomerName(Domain.Entities.Profile.Users? user)
    {
        if (user == null)
            return string.Empty;

        var fullName = ((user.Firstname ?? "") + " " + (user.Lastname ?? "")).Trim();
        return string.IsNullOrWhiteSpace(fullName) ? user.Username ?? string.Empty : fullName;
    }

    private bool IsTenantActor(Guid tenantId)
        => userContext.TenantId.HasValue && userContext.TenantId.Value == tenantId;

    private bool IsCustomerActor(Review review)
        => userContext.UserId.HasValue && review.FkCustomerId == userContext.UserId.Value;

    private sealed record SourceValidationResult(bool IsValid, Guid TenantId, bool IsVerified, string? ErrorMessage)
    {
        public static SourceValidationResult Success(Guid tenantId, bool isVerified)
            => new(true, tenantId, isVerified, null);

        public static SourceValidationResult Fail(string error)
            => new(false, Guid.Empty, false, error);
    }
}
