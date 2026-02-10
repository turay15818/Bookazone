using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Booking;
using Bookazone.Application.DTOs.Converter;
using Bookazone.Application.DTOs.Response;
using Bookazone.Application.Interfaces.Context;
using Bookazone.Application.Interfaces.Repository.Booking;
using Bookazone.Application.Interfaces.Repository.Sports;
using Bookazone.Application.Interfaces.Repository.Subscription;
using Bookazone.Application.Interfaces.Repository.Tenant;
using Bookazone.Application.Interfaces.Services.Booking;
using Bookazone.Domain.Entities.Booking;
using Bookazone.Domain.Entities.Sports;
using Bookazone.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using BookingEntity = Bookazone.Domain.Entities.Booking.Booking;

namespace Bookazone.Application.Services.Booking;

public class BookingService(
    IBookingRepository bookingRepository,
    IBookingApprovalRepository bookingApprovalRepository,
    ISportResourcePricingRuleRepository sportResourcePricingRuleRepository,
    ITenantWorkingHourRepository tenantWorkingHourRepository,
    ITenantSettingsRepository tenantSettingsRepository,
    IUserContext userContext,
    IBookingValidationService bookingValidationService,
    ILogger<BookingService> logger)
    : IBookingService
{
    public async Task<ApiResult> CreateAsync(BookingCreateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var customerId = userContext.UserId;
            if (customerId == null)
                return ApiResponse.Error(ErrorHttp.Forbidden,
                    new Exception(" user context does not contain a valid user id."));

            var validation = await bookingValidationService.ValidateBookingAsync(
                new BookingValidationRequest
                {
                    TenantId = request.TenantId,
                    ResourceId = request.ResourceId,
                    StartUtc = request.StartUtc,
                    EndUtc = request.EndUtc,
                    ExpectedAttendees = request.ExpectedAttendees
                },
                cancellationToken);

            if (!validation.IsValid)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation.Message);

            var pricing = await CalculatePriceAsync(
                request.TenantId,
                request.ResourceId,
                request.StartUtc,
                request.EndUtc,
                cancellationToken);

            if (!pricing.IsValid)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, pricing.ErrorMessage ?? "Pricing is not configured.");

            var booking = new BookingEntity
            {
                FkTenantId = request.TenantId,
                FkSportResourceId = request.ResourceId,
                FkCustomerId = customerId ?? Guid.Empty,
                StartUtc = NormalizeUtc(request.StartUtc),
                EndUtc = NormalizeUtc(request.EndUtc),
                ExpectedAttendees = request.ExpectedAttendees,
                BookingMode = BookingMode.Approval,
                Status = BookingStatus.PendingApproval,
                TotalPrice = pricing.TotalPrice,
                Currency = pricing.Currency,
                Notes = request.Notes
            };

            var approval = new BookingApproval
            {
                FkBookingId = booking.Id,
                Decision = BookingApprovalDecision.Pending
            };

            await using var transaction = await bookingRepository.BeginTransactionAsync();
            var created = await bookingRepository.AddAsync(booking);
            await bookingApprovalRepository.AddAsync(approval);
            await transaction.CommitAsync();

            return ApiResponse.Success(BookingConverter.ToDto(created));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating booking");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpdateAsync(BookingUpdateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await GetBookingForUpdateAsync(request.BookingId, cancellationToken);
            if (existing == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (existing.FkTenantId != request.TenantId)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Tenant mismatch for booking update.");

            if (existing.Status is BookingStatus.Cancelled or BookingStatus.Declined or BookingStatus.Completed)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Booking cannot be updated in its current status.");

            var validation = await bookingValidationService.ValidateBookingAsync(
                new BookingValidationRequest
                {
                    BookingId = existing.Id,
                    TenantId = request.TenantId,
                    ResourceId = request.ResourceId,
                    StartUtc = request.StartUtc,
                    EndUtc = request.EndUtc,
                    ExpectedAttendees = request.ExpectedAttendees
                },
                cancellationToken);

            if (!validation.IsValid)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation.Message);

            var pricing = await CalculatePriceAsync(
                request.TenantId,
                request.ResourceId,
                request.StartUtc,
                request.EndUtc,
                cancellationToken);

            if (!pricing.IsValid)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, pricing.ErrorMessage ?? "Pricing is not configured.");

            existing.FkSportResourceId = request.ResourceId;
            existing.StartUtc = NormalizeUtc(request.StartUtc);
            existing.EndUtc = NormalizeUtc(request.EndUtc);
            existing.ExpectedAttendees = request.ExpectedAttendees;
            existing.TotalPrice = pricing.TotalPrice;
            existing.Currency = pricing.Currency;
            existing.Notes = request.Notes;
            existing.BookingMode = BookingMode.Approval;
            existing.Status = BookingStatus.PendingApproval;

            await using var transaction = await bookingRepository.BeginTransactionAsync();
            var updated = await bookingRepository.UpdateAsync(existing);
            await ResetApprovalAsync(existing.Id, cancellationToken);
            await transaction.CommitAsync();
            return ApiResponse.Success(BookingConverter.ToDto(updated));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating booking {BookingId}", request.BookingId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> ApproveAsync(BookingDecisionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var booking = await GetBookingForUpdateAsync(request.BookingId, cancellationToken);
            if (booking == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!HasUserContext())
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User context does not contain a valid user id."));

            if (!IsTenantActor(booking.FkTenantId))
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User is not authorized for this tenant."));

            if (booking.Status != BookingStatus.PendingApproval)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Booking is not awaiting approval.");

            var approval = await GetOrCreateApprovalAsync(booking.Id, cancellationToken);
            approval.Decision = BookingApprovalDecision.Approved;
            approval.DecisionAtUtc = DateTime.UtcNow;
            approval.Reason = NormalizeReason(request.Reason);

            booking.Status = BookingStatus.Confirmed;
            booking.BookingMode = BookingMode.Approval;

            await using var transaction = await bookingRepository.BeginTransactionAsync();
            var updated = await bookingRepository.UpdateAsync(booking);
            await bookingApprovalRepository.UpdateAsync(approval);
            await transaction.CommitAsync();

            return ApiResponse.Success(BookingConverter.ToDto(updated));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error approving booking {BookingId}", request.BookingId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> DeclineAsync(BookingDecisionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var booking = await GetBookingForUpdateAsync(request.BookingId, cancellationToken);
            if (booking == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!HasUserContext())
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User context does not contain a valid user id."));

            if (!IsTenantActor(booking.FkTenantId))
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User is not authorized for this tenant."));

            if (booking.Status != BookingStatus.PendingApproval)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Booking is not awaiting approval.");

            var approval = await GetOrCreateApprovalAsync(booking.Id, cancellationToken);
            approval.Decision = BookingApprovalDecision.Declined;
            approval.DecisionAtUtc = DateTime.UtcNow;
            approval.Reason = NormalizeReason(request.Reason);

            booking.Status = BookingStatus.Declined;
            booking.BookingMode = BookingMode.Approval;

            await using var transaction = await bookingRepository.BeginTransactionAsync();
            var updated = await bookingRepository.UpdateAsync(booking);
            await bookingApprovalRepository.UpdateAsync(approval);
            await transaction.CommitAsync();

            return ApiResponse.Success(BookingConverter.ToDto(updated));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error declining booking {BookingId}", request.BookingId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> CancelAsync(BookingCancelRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var booking = await GetBookingForUpdateAsync(request.BookingId, cancellationToken);
            if (booking == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!HasUserContext())
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User context does not contain a valid user id."));

            if (!IsTenantActor(booking.FkTenantId) && !IsCustomerActor(booking))
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User is not authorized to cancel this booking."));

            if (booking.Status is BookingStatus.Cancelled or BookingStatus.Declined or BookingStatus.Completed)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Booking cannot be cancelled in its current status.");

            var approval = await GetOrCreateApprovalAsync(booking.Id, cancellationToken);
            approval.Decision = BookingApprovalDecision.Cancelled;
            approval.DecisionAtUtc = DateTime.UtcNow;
            approval.Reason = NormalizeReason(request.Reason);

            booking.Status = BookingStatus.Cancelled;
            booking.BookingMode = BookingMode.Approval;

            await using var transaction = await bookingRepository.BeginTransactionAsync();
            var updated = await bookingRepository.UpdateAsync(booking);
            await bookingApprovalRepository.UpdateAsync(approval);
            await transaction.CommitAsync();

            return ApiResponse.Success(BookingConverter.ToDto(updated));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cancelling booking {BookingId}", request.BookingId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> RescheduleAsync(BookingRescheduleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var booking = await GetBookingForUpdateAsync(request.BookingId, cancellationToken);
            if (booking == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!HasUserContext())
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User context does not contain a valid user id."));

            if (!IsTenantActor(booking.FkTenantId) && !IsCustomerActor(booking))
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User is not authorized to reschedule this booking."));

            if (booking.Status is BookingStatus.Cancelled or BookingStatus.Declined or BookingStatus.Completed)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Booking cannot be rescheduled in its current status.");

            var validation = await bookingValidationService.ValidateBookingAsync(
                new BookingValidationRequest
                {
                    BookingId = booking.Id,
                    TenantId = booking.FkTenantId,
                    ResourceId = booking.FkSportResourceId,
                    StartUtc = request.StartUtc,
                    EndUtc = request.EndUtc,
                    ExpectedAttendees = request.ExpectedAttendees
                },
                cancellationToken);

            if (!validation.IsValid)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation.Message);

            var pricing = await CalculatePriceAsync(
                booking.FkTenantId,
                booking.FkSportResourceId,
                request.StartUtc,
                request.EndUtc,
                cancellationToken);

            if (!pricing.IsValid)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, pricing.ErrorMessage ?? "Pricing is not configured.");

            booking.StartUtc = NormalizeUtc(request.StartUtc);
            booking.EndUtc = NormalizeUtc(request.EndUtc);
            booking.ExpectedAttendees = request.ExpectedAttendees;
            booking.TotalPrice = pricing.TotalPrice;
            booking.Currency = pricing.Currency;
            booking.Notes = request.Notes;
            booking.BookingMode = BookingMode.Approval;
            booking.Status = BookingStatus.PendingApproval;

            await using var transaction = await bookingRepository.BeginTransactionAsync();
            var updated = await bookingRepository.UpdateAsync(booking);
            await ResetApprovalAsync(booking.Id, cancellationToken);
            await transaction.CommitAsync();

            return ApiResponse.Success(BookingConverter.ToDto(updated));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error rescheduling booking {BookingId}", request.BookingId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetTenantBookingsAsync(
        List<BookingStatus>? statuses = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        Guid? resourceId = null,
        Guid? customerId = null,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!HasUserContext() || !userContext.TenantId.HasValue)
                return ApiResponse.Error(ErrorHttp.Forbidden,
                    new Exception("User context does not contain a valid tenant id."));

            var tenantId = userContext.TenantId.Value;

            var query = bookingRepository.Query()
                .AsNoTracking()
                .Where(b =>
                    b.FkTenantId == tenantId &&
                    b.Active &&
                    !b.Deleted);

            if (resourceId.HasValue)
                query = query.Where(b => b.FkSportResourceId == resourceId.Value);

            if (customerId.HasValue)
                query = query.Where(b => b.FkCustomerId == customerId.Value);

            query = ApplyRangeFilter(query, fromUtc, toUtc);

            if (statuses is { Count: > 0 })
                query = query.Where(b => statuses.Contains(b.Status));

            var projected = query
                .Select(b => new VwBookingListItem
                {
                    Id = b.Id,
                    TenantId = b.FkTenantId,
                    TenantName = b.FkTenant.Name,
                    ResourceId = b.FkSportResourceId,
                    ResourceName = b.FkSportResource.Name,
                    SportTypeId = b.FkSportResource.FkTenantSportTypeId,
                    SportTypeName = b.FkSportResource.FkTenantSportType.Name,
                    CustomerId = b.FkCustomerId,
                    CustomerName = ((b.FkCustomer.Firstname ?? "") + " " + (b.FkCustomer.Lastname ?? "")).Trim(),
                    CustomerEmail = b.FkCustomer.Email,
                    Status = b.Status,
                    BookingMode = b.BookingMode,
                    StartUtc = b.StartUtc,
                    EndUtc = b.EndUtc,
                    ExpectedAttendees = b.ExpectedAttendees,
                    TotalPrice = b.TotalPrice,
                    Currency = b.Currency,
                    Notes = b.Notes
                });

            var sorted = ApplySorting(projected, sortBy, sortDirection);
            var paged = await Paginate<VwBookingListItem>.CreateAsync(sorted, pageIndex, pageSize);

            return ApiResponse.Success(paged);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant bookings");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetCustomerBookingsAsync(
        List<BookingStatus>? statuses = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        Guid? tenantId = null,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!HasUserContext() || !userContext.UserId.HasValue)
                return ApiResponse.Error(ErrorHttp.Forbidden,
                    new Exception("User context does not contain a valid user id."));

            var customerId = userContext.UserId.Value;

            var query = bookingRepository.Query()
                .AsNoTracking()
                .Where(b =>
                    b.FkCustomerId == customerId &&
                    b.Active &&
                    !b.Deleted);

            if (tenantId.HasValue)
                query = query.Where(b => b.FkTenantId == tenantId.Value);

            query = ApplyRangeFilter(query, fromUtc, toUtc);

            if (statuses is { Count: > 0 })
                query = query.Where(b => statuses.Contains(b.Status));

            var projected = query
                .Select(b => new VwBookingListItem
                {
                    Id = b.Id,
                    TenantId = b.FkTenantId,
                    TenantName = b.FkTenant.Name,
                    ResourceId = b.FkSportResourceId,
                    ResourceName = b.FkSportResource.Name,
                    SportTypeId = b.FkSportResource.FkTenantSportTypeId,
                    SportTypeName = b.FkSportResource.FkTenantSportType.Name,
                    CustomerId = b.FkCustomerId,
                    CustomerName = ((b.FkCustomer.Firstname ?? "") + " " + (b.FkCustomer.Lastname ?? "")).Trim(),
                    CustomerEmail = b.FkCustomer.Email,
                    Status = b.Status,
                    BookingMode = b.BookingMode,
                    StartUtc = b.StartUtc,
                    EndUtc = b.EndUtc,
                    ExpectedAttendees = b.ExpectedAttendees,
                    TotalPrice = b.TotalPrice,
                    Currency = b.Currency,
                    Notes = b.Notes
                });

            var sorted = ApplySorting(projected, sortBy, sortDirection);
            var paged = await Paginate<VwBookingListItem>.CreateAsync(sorted, pageIndex, pageSize);

            return ApiResponse.Success(paged);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting customer bookings");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    private bool HasUserContext()
        => userContext.UserId.HasValue;

    private bool IsTenantActor(Guid tenantId)
        => userContext.TenantId.HasValue && userContext.TenantId.Value == tenantId;

    private bool IsCustomerActor(BookingEntity booking)
        => userContext.UserId.HasValue && booking.FkCustomerId == userContext.UserId.Value;

    private static string? NormalizeReason(string? reason)
        => string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();

    private static IQueryable<BookingEntity> ApplyRangeFilter(
        IQueryable<BookingEntity> query,
        DateTime? fromUtc,
        DateTime? toUtc)
    {
        if (fromUtc.HasValue)
            query = query.Where(b => b.EndUtc > fromUtc.Value);

        if (toUtc.HasValue)
            query = query.Where(b => b.StartUtc < toUtc.Value);

        return query;
    }

    private static IQueryable<VwBookingListItem> ApplySorting(
        IQueryable<VwBookingListItem> query,
        string? sortBy,
        string? sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var key = sortBy?.Trim().ToLowerInvariant();

        return key switch
        {
            "startutc" or "start" => descending ? query.OrderByDescending(x => x.StartUtc) : query.OrderBy(x => x.StartUtc),
            "endutc" or "end" => descending ? query.OrderByDescending(x => x.EndUtc) : query.OrderBy(x => x.EndUtc),
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            "price" or "totalprice" => descending ? query.OrderByDescending(x => x.TotalPrice) : query.OrderBy(x => x.TotalPrice),
            "tenant" or "tenantname" => descending ? query.OrderByDescending(x => x.TenantName) : query.OrderBy(x => x.TenantName),
            "resource" or "resourcename" => descending ? query.OrderByDescending(x => x.ResourceName) : query.OrderBy(x => x.ResourceName),
            "customer" or "customername" => descending ? query.OrderByDescending(x => x.CustomerName) : query.OrderBy(x => x.CustomerName),
            _ => query.OrderByDescending(x => x.StartUtc)
        };
    }

    private Task<BookingEntity?> GetBookingForUpdateAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        return bookingRepository.Query()
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(b => b.Id == bookingId && !b.Deleted, cancellationToken);
    }

    private Task<BookingApproval?> GetApprovalForUpdateAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        return bookingApprovalRepository.Query()
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(a =>
                a.FkBookingId == bookingId &&
                a.Active &&
                !a.Deleted,
                cancellationToken);
    }

    private async Task ResetApprovalAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        var approval = await GetApprovalForUpdateAsync(bookingId, cancellationToken);
        if (approval == null)
        {
            await bookingApprovalRepository.AddAsync(new BookingApproval
            {
                FkBookingId = bookingId,
                Decision = BookingApprovalDecision.Pending
            });
            return;
        }

        approval.Decision = BookingApprovalDecision.Pending;
        approval.DecisionAtUtc = null;
        approval.Reason = null;
        await bookingApprovalRepository.UpdateAsync(approval);
    }

    private async Task<BookingApproval> GetOrCreateApprovalAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        var approval = await GetApprovalForUpdateAsync(bookingId, cancellationToken);
        if (approval != null)
            return approval;

        var created = new BookingApproval
        {
            FkBookingId = bookingId,
            Decision = BookingApprovalDecision.Pending
        };

        return await bookingApprovalRepository.AddAsync(created);
    }

    private async Task<PricingResult> CalculatePriceAsync(
        Guid tenantId,
        Guid resourceId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken)
    {
        if (endUtc <= startUtc)
            return PricingResult.Fail("End time must be greater than start time.");

        var rules = await sportResourcePricingRuleRepository.GetByResourceAsync(resourceId, cancellationToken);
        if (rules.Count == 0)
            return PricingResult.Fail("Pricing rules are not configured for this resource.");

        var localRange = await GetLocalRangeAsync(tenantId, startUtc, endUtc, cancellationToken);
        if (localRange.StartLocal.Date != localRange.EndLocal.Date)
            return PricingResult.Fail("Booking must be within a single local day for pricing.");

        var applicableRules = rules
            .Where(r => IsRuleApplicable(r, localRange.StartLocal, localRange.EndLocal))
            .ToList();

        if (applicableRules.Count == 0)
            return PricingResult.Fail("No pricing rule matches this booking time.");

        var rule = applicableRules
            .OrderByDescending(GetRuleSpecificity)
            .ThenBy(r => r.StartTime ?? TimeSpan.Zero)
            .First();

        var durationMinutes = (int)(localRange.EndLocal - localRange.StartLocal).TotalMinutes;
        if (rule.MinDurationMinutes.HasValue && durationMinutes < rule.MinDurationMinutes.Value)
            return PricingResult.Fail($"Minimum duration is {rule.MinDurationMinutes.Value} minutes.");

        decimal total;
        switch (rule.RuleType)
        {
            case PricingRuleType.Flat:
                total = rule.PriceAmount;
                break;
            case PricingRuleType.PerHour:
                total = rule.PriceAmount * (durationMinutes / 60m);
                break;
            case PricingRuleType.PerSlot:
            {
                var workingHour = await tenantWorkingHourRepository.GetByTenantAndDayAsync(
                    tenantId,
                    localRange.StartLocal.DayOfWeek,
                    cancellationToken);
                if (workingHour == null || workingHour.SlotDurationMinutes <= 0)
                    return PricingResult.Fail("Slot duration is not configured for this tenant.");

                if (durationMinutes % workingHour.SlotDurationMinutes != 0)
                    return PricingResult.Fail("Booking duration does not align with slot duration.");

                var slotCount = durationMinutes / workingHour.SlotDurationMinutes;
                total = rule.PriceAmount * slotCount;
                break;
            }
            default:
                return PricingResult.Fail("Unsupported pricing rule type.");
        }

        var currency = NormalizeCurrency(rule.Currency);
        var rounded = Math.Round(total, 2, MidpointRounding.AwayFromZero);
        return PricingResult.Success(rounded, currency);
    }

    private static bool IsRuleApplicable(SportResourcePricingRule rule, DateTime startLocal, DateTime endLocal)
    {
        if (rule.DayOfWeek.HasValue && rule.DayOfWeek.Value != startLocal.DayOfWeek)
            return false;

        if (rule.StartTime.HasValue && startLocal.TimeOfDay < rule.StartTime.Value)
            return false;

        if (rule.EndTime.HasValue && endLocal.TimeOfDay > rule.EndTime.Value)
            return false;

        return true;
    }

    private static int GetRuleSpecificity(SportResourcePricingRule rule)
    {
        var score = 0;
        if (rule.DayOfWeek.HasValue) score += 2;
        if (rule.StartTime.HasValue || rule.EndTime.HasValue) score += 1;
        if (rule.MinDurationMinutes.HasValue) score += 1;
        return score;
    }

    private async Task<LocalRange> GetLocalRangeAsync(
        Guid tenantId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken)
    {
        var timeZone = await ResolveTenantTimeZoneAsync(tenantId);
        var normalizedStart = NormalizeUtc(startUtc);
        var normalizedEnd = NormalizeUtc(endUtc);

        return new LocalRange(
            TimeZoneInfo.ConvertTimeFromUtc(normalizedStart, timeZone),
            TimeZoneInfo.ConvertTimeFromUtc(normalizedEnd, timeZone));
    }

    private async Task<TimeZoneInfo> ResolveTenantTimeZoneAsync(Guid tenantId)
    {
        var settings = await tenantSettingsRepository.GetByTenantAsync(tenantId);
        if (string.IsNullOrWhiteSpace(settings?.DefaultTimeZone))
            return TimeZoneInfo.Utc;

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(settings.DefaultTimeZone);
        }
        catch (TimeZoneNotFoundException ex)
        {
            logger.LogWarning(ex, "Time zone not found for tenant {TenantId}: {TimeZoneId}", tenantId, settings.DefaultTimeZone);
            return TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException ex)
        {
            logger.LogWarning(ex, "Invalid time zone for tenant {TenantId}: {TimeZoneId}", tenantId, settings.DefaultTimeZone);
            return TimeZoneInfo.Utc;
        }
    }

    private sealed record LocalRange(DateTime StartLocal, DateTime EndLocal);

    private sealed record PricingResult(bool IsValid, decimal TotalPrice, string Currency, string? ErrorMessage)
    {
        public static PricingResult Success(decimal totalPrice, string currency)
            => new(true, totalPrice, currency, null);

        public static PricingResult Fail(string error)
            => new(false, 0m, string.Empty, error);
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    private static string NormalizeCurrency(string? currency)
    {
        return string.IsNullOrWhiteSpace(currency)
            ? "USD"
            : currency.Trim().ToUpperInvariant();
    }
}
