using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Response;
using Bookazone.Application.DTOs.Converter;
using Bookazone.Application.DTOs.Rentals;
using Bookazone.Application.Interfaces.Context;
using Bookazone.Application.Interfaces.Repository.Booking;
using Bookazone.Application.Interfaces.Repository.Rentals;
using Bookazone.Application.Interfaces.Repository.Subscription;
using Bookazone.Application.Interfaces.Services.Rentals;
using Bookazone.Domain.Entities.Rentals;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Services.Rentals;

public class RentalOrderService(
    IRentalOrderRepository rentalOrderRepository,
    IRentalRepository rentalRepository,
    IRentalPricingRuleRepository rentalPricingRuleRepository,
    ITenantBookingCategoryRepository tenantBookingCategoryRepository,
    ITenantSettingsRepository tenantSettingsRepository,
    IUserContext userContext,
    IHttpContextAccessor httpContextAccessor,
    ILogger<RentalOrderService> logger)
    : IRentalOrderService
{
    public async Task<ApiResult> CreateAsync(RentalOrderCreateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!userContext.UserId.HasValue)
                return ApiResponse.Error(ErrorHttp.Forbidden,
                    new Exception("User context does not contain a valid user id."));

            var rental = await rentalRepository.Query()
                .AsNoTracking()
                .Include(v => v.FkCoverMedia)
                .ThenInclude(m => m.FkStoredFile)
                .FirstOrDefaultAsync(v =>
                        v.Id == request.RentalId &&
                        v.Active &&
                        !v.Deleted &&
                        v.Status == RentalStatus.Published,
                    cancellationToken);

            if (rental == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!await TenantSupportsRentalsAsync(rental.FkTenantId, cancellationToken))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Tenant is not enabled for rentals.");

            var validationError = ValidateRequest(request);
            if (!string.IsNullOrWhiteSpace(validationError))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validationError);

            var startUtc = NormalizeUtc(request.StartUtc);
            var endUtc = NormalizeUtc(request.EndUtc);

            if (!TryResolveQuantity(
                    request.PricingUnit,
                    request.Quantity,
                    startUtc,
                    endUtc,
                    out var quantity,
                    out var quantityError))
            {
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, quantityError ?? "Invalid quantity.");
            }

            var unitsRequested = request.UnitsRequested.HasValue && request.UnitsRequested.Value > 0
                ? request.UnitsRequested.Value
                : 1;

            if (unitsRequested <= 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "UnitsRequested must be greater than zero.");

            if (rental.Capacity.HasValue &&
                request.GuestCount.HasValue &&
                request.GuestCount.Value > rental.Capacity.Value)
            {
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "GuestCount exceeds the rental capacity.");
            }

            var availabilityError = await ValidateAvailabilityAsync(
                rental,
                startUtc,
                endUtc,
                request.PricingUnit,
                unitsRequested,
                cancellationToken);
            if (!string.IsNullOrWhiteSpace(availabilityError))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, availabilityError);

            var pricingRule = await ResolvePricingRuleAsync(rental.Id, request.PricingUnit, quantity, cancellationToken);
            if (pricingRule == null)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Pricing rule not configured for this unit/quantity.");

            var unitPrice = pricingRule.PriceAmount;
            var totalPrice = Math.Round(unitPrice * quantity, 2, MidpointRounding.AwayFromZero);
            var currency = NormalizeCurrency(pricingRule.Currency);
            var status = rental.BookingMode == BookingMode.Instant
                ? RentalOrderStatus.Confirmed
                : RentalOrderStatus.Pending;

            var order = new RentalOrder
            {
                FkRentalId = rental.Id,
                FkTenantId = rental.FkTenantId,
                FkCustomerId = userContext.UserId.Value,
                Status = status,
                PricingUnit = request.PricingUnit,
                Quantity = quantity,
                UnitsRequested = unitsRequested,
                UnitPrice = unitPrice,
                TotalPrice = totalPrice,
                Currency = currency,
                StartUtc = startUtc,
                EndUtc = endUtc,
                GuestCount = request.GuestCount,
                ContactName = NormalizeText(request.ContactName, 150),
                ContactEmail = NormalizeText(request.ContactEmail, 200),
                ContactPhone = NormalizeText(request.ContactPhone, 50),
                Notes = NormalizeText(request.Notes, 2000),
                Active = true,
                Deleted = false
            };

            var created = await rentalOrderRepository.AddAsync(order);
            created.FkRental = rental;
            return ApiResponse.Success(ToDetail(created, rental, null));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating rental order");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await LoadOrderAsync(orderId, cancellationToken);
            if (order == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!IsTenantActor(order.FkTenantId) && !IsCustomerActor(order))
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User is not authorized to view this order."));

            return ApiResponse.Success(ToDetail(order, order.FkRental, order.FkTenant));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting rental order {OrderId}", orderId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetCustomerOrdersAsync(
        List<RentalOrderStatus>? statuses = null,
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
            if (!userContext.UserId.HasValue)
                return ApiResponse.Error(ErrorHttp.Forbidden,
                    new Exception("User context does not contain a valid user id."));

            var customerId = userContext.UserId.Value;

            var query = rentalOrderRepository.Query()
                .AsNoTracking()
                .Where(o =>
                    o.FkCustomerId == customerId &&
                    o.Active &&
                    !o.Deleted);

            if (tenantId.HasValue)
                query = query.Where(o => o.FkTenantId == tenantId.Value);

            query = ApplyRangeFilter(query, fromUtc, toUtc);

            if (statuses is { Count: > 0 })
                query = query.Where(o => statuses.Contains(o.Status));

            var projected = query.Select(o => new VwRentalOrderListItem
            {
                Id = o.Id,
                RentalId = o.FkRentalId,
                RentalTitle = o.FkRental.Title,
                RentalType = o.FkRental.Type,
                RentalCoverUrl = o.FkRental.FkCoverMedia != null
                    ? o.FkRental.FkCoverMedia.FkStoredFile.RelativePath
                    : null,
                TenantId = o.FkTenantId,
                TenantName = o.FkTenant.Name,
                CustomerId = o.FkCustomerId,
                CustomerName = ((o.FkCustomer.Firstname ?? "") + " " + (o.FkCustomer.Lastname ?? "")).Trim(),
                CustomerEmail = o.FkCustomer.Email,
                Status = o.Status,
                PricingUnit = o.PricingUnit,
                Quantity = o.Quantity,
                UnitsRequested = o.UnitsRequested,
                UnitPrice = o.UnitPrice,
                TotalPrice = o.TotalPrice,
                Currency = o.Currency,
                StartUtc = o.StartUtc,
                EndUtc = o.EndUtc,
                DateCreated = o.DateCreated
            });

            var sorted = ApplySorting(projected, sortBy, sortDirection);
            var paged = await Paginate<VwRentalOrderListItem>.CreateAsync(sorted, pageIndex, pageSize);

            var request = httpContextAccessor.HttpContext?.Request;
            foreach (var item in paged.Data)
                item.RentalCoverUrl = FileUrlHelper.BuildPublicUrl(request, item.RentalCoverUrl);

            return ApiResponse.Success(paged);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting customer rental orders");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetTenantOrdersAsync(
        List<RentalOrderStatus>? statuses = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        Guid? RentalId = null,
        Guid? customerId = null,
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

            var query = rentalOrderRepository.Query()
                .AsNoTracking()
                .Where(o =>
                    o.FkTenantId == tenantId &&
                    o.Active &&
                    !o.Deleted);

            if (RentalId.HasValue)
                query = query.Where(o => o.FkRentalId == RentalId.Value);

            if (customerId.HasValue)
                query = query.Where(o => o.FkCustomerId == customerId.Value);

            query = ApplyRangeFilter(query, fromUtc, toUtc);

            if (statuses is { Count: > 0 })
                query = query.Where(o => statuses.Contains(o.Status));

            var projected = query.Select(o => new VwRentalOrderListItem
            {
                Id = o.Id,
                RentalId = o.FkRentalId,
                RentalTitle = o.FkRental.Title,
                RentalType = o.FkRental.Type,
                RentalCoverUrl = o.FkRental.FkCoverMedia != null
                    ? o.FkRental.FkCoverMedia.FkStoredFile.RelativePath
                    : null,
                TenantId = o.FkTenantId,
                TenantName = o.FkTenant.Name,
                CustomerId = o.FkCustomerId,
                CustomerName = ((o.FkCustomer.Firstname ?? "") + " " + (o.FkCustomer.Lastname ?? "")).Trim(),
                CustomerEmail = o.FkCustomer.Email,
                Status = o.Status,
                PricingUnit = o.PricingUnit,
                Quantity = o.Quantity,
                UnitsRequested = o.UnitsRequested,
                UnitPrice = o.UnitPrice,
                TotalPrice = o.TotalPrice,
                Currency = o.Currency,
                StartUtc = o.StartUtc,
                EndUtc = o.EndUtc,
                DateCreated = o.DateCreated
            });

            var sorted = ApplySorting(projected, sortBy, sortDirection);
            var paged = await Paginate<VwRentalOrderListItem>.CreateAsync(sorted, pageIndex, pageSize);

            var request = httpContextAccessor.HttpContext?.Request;
            foreach (var item in paged.Data)
                item.RentalCoverUrl = FileUrlHelper.BuildPublicUrl(request, item.RentalCoverUrl);

            return ApiResponse.Success(paged);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant rental orders");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpdateStatusAsync(RentalOrderStatusUpdateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await GetOrderForUpdateAsync(request.OrderId, cancellationToken);
            if (order == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!IsTenantActor(order.FkTenantId))
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User is not authorized for this tenant."));

            if (!ValidateStatusTransition(order.Status, request.Status))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Invalid status transition.");

            order.Status = request.Status;
            var updated = await rentalOrderRepository.UpdateAsync(order);
            return ApiResponse.Success(await ToDetailAsync(updated, cancellationToken));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating rental order status {OrderId}", request.OrderId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> CancelAsync(RentalOrderCancelRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await GetOrderForUpdateAsync(request.OrderId, cancellationToken);
            if (order == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!IsTenantActor(order.FkTenantId) && !IsCustomerActor(order))
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User is not authorized to cancel this order."));

            if (order.Status is RentalOrderStatus.Cancelled or RentalOrderStatus.Declined or RentalOrderStatus.Completed)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Order cannot be cancelled in its current status.");

            order.Status = RentalOrderStatus.Cancelled;
            var updated = await rentalOrderRepository.UpdateAsync(order);
            return ApiResponse.Success(await ToDetailAsync(updated, cancellationToken));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cancelling rental order {OrderId}", request.OrderId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    private async Task<RentalOrder?> LoadOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await rentalOrderRepository.Query()
            .AsNoTracking()
            .Include(o => o.FkRental)
            .ThenInclude(v => v.FkCoverMedia)
            .ThenInclude(m => m.FkStoredFile)
            .Include(o => o.FkTenant)
            .Include(o => o.FkCustomer)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.Active && !o.Deleted, cancellationToken);
    }

    private Task<RentalOrder?> GetOrderForUpdateAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return rentalOrderRepository.Query()
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.Deleted, cancellationToken);
    }

    private async Task<VwRentalOrderDetail> ToDetailAsync(RentalOrder order, CancellationToken cancellationToken)
    {
        var loaded = await LoadOrderAsync(order.Id, cancellationToken) ?? order;
        return ToDetail(loaded, loaded.FkRental, loaded.FkTenant);
    }

    private VwRentalOrderDetail ToDetail(RentalOrder order, Rental? rental, Domain.Entities.Tenant.Tenants? tenant)
    {
        var coverUrl = rental?.FkCoverMedia?.FkStoredFile?.RelativePath;
        var request = httpContextAccessor.HttpContext?.Request;
        coverUrl = FileUrlHelper.BuildPublicUrl(request, coverUrl);

        var customerName = ((order.FkCustomer?.Firstname ?? "") + " " + (order.FkCustomer?.Lastname ?? "")).Trim();
        var rentalType = rental?.Type ?? RentalType.Hall;
        return RentalOrderConverter.ToDetail(
            order,
            coverUrl,
            customerName,
            order.FkCustomer?.Email,
            tenant?.Name,
            rental?.Title,
            rentalType);
    }

    private async Task<RentalPricingRule?> ResolvePricingRuleAsync(
        Guid rentalId,
        RentalPricingUnit unit,
        decimal quantity,
        CancellationToken cancellationToken)
    {
        var rules = await rentalPricingRuleRepository.GetByRentalAsync(rentalId, cancellationToken);
        var applicable = rules
            .Where(r => r.Unit == unit && IsQuantityInRange(r, quantity))
            .ToList();

        if (applicable.Count == 0)
            return null;

        return applicable.FirstOrDefault(r => r.IsPrimary)
               ?? applicable.OrderBy(r => r.PriceAmount).First();
    }

    private static bool IsQuantityInRange(RentalPricingRule rule, decimal quantity)
    {
        var min = rule.MinQuantity.GetValueOrDefault();
        if (rule.MinQuantity.HasValue && min > 0 && quantity < min)
            return false;
        var max = rule.MaxQuantity.GetValueOrDefault();
        if (rule.MaxQuantity.HasValue && max > 0 && quantity > max)
            return false;
        return true;
    }

    private static bool TryResolveQuantity(
        RentalPricingUnit unit,
        decimal? requestedQuantity,
        DateTime? startUtc,
        DateTime? endUtc,
        out decimal quantity,
        out string? error)
    {
        quantity = 0m;
        error = null;

        if (!startUtc.HasValue || !endUtc.HasValue)
        {
            error = "StartUtc and EndUtc are required for rentals.";
            return false;
        }

        if (endUtc.Value <= startUtc.Value)
        {
            error = "EndUtc must be greater than StartUtc.";
            return false;
        }

        var duration = endUtc.Value - startUtc.Value;
        quantity = unit switch
        {
            RentalPricingUnit.Hour => (decimal)duration.TotalHours,
            RentalPricingUnit.Day => (decimal)duration.TotalDays,
            RentalPricingUnit.Week => (decimal)(duration.TotalDays / 7d),
            RentalPricingUnit.Month => (decimal)(duration.TotalDays / 30d),
            RentalPricingUnit.Year => (decimal)(duration.TotalDays / 365d),
            _ => 0m
        };

        if (requestedQuantity.HasValue && requestedQuantity.Value > 0)
            quantity = requestedQuantity.Value;

        if (quantity <= 0)
        {
            error = "Quantity must be greater than zero.";
            return false;
        }

        return true;
    }

    private async Task<string?> ValidateAvailabilityAsync(
        Rental rental,
        DateTime? startUtc,
        DateTime? endUtc,
        RentalPricingUnit pricingUnit,
        int unitsRequested,
        CancellationToken cancellationToken)
    {
        if (!startUtc.HasValue || !endUtc.HasValue)
            return "StartUtc and EndUtc are required for rentals.";

        if (unitsRequested <= 0)
            return "UnitsRequested must be greater than zero.";

        if (rental.UnitsAvailable <= 0)
            return "No units available for this rental.";

        if (unitsRequested > rental.UnitsAvailable)
            return "Requested units exceed available inventory.";

        var timeZone = await ResolveTenantTimeZoneAsync(rental.FkTenantId);
        var effectiveStartUtc = startUtc.Value;
        var effectiveEndUtc = ResolveEffectiveEndUtc(pricingUnit, startUtc.Value, endUtc.Value, timeZone);

        var activeOrders = await rentalOrderRepository.Query()
            .AsNoTracking()
            .Where(o =>
                o.FkRentalId == rental.Id &&
                o.Active &&
                !o.Deleted &&
                o.Status != RentalOrderStatus.Cancelled &&
                o.Status != RentalOrderStatus.Declined &&
                o.Status != RentalOrderStatus.Completed &&
                (o.StartUtc == null ||
                 o.EndUtc == null ||
                 (o.StartUtc < effectiveEndUtc && o.EndUtc > effectiveStartUtc) ||
                 ((o.PricingUnit == RentalPricingUnit.Week ||
                   o.PricingUnit == RentalPricingUnit.Month ||
                   o.PricingUnit == RentalPricingUnit.Year) &&
                  o.StartUtc < effectiveEndUtc)))
            .ToListAsync(cancellationToken);

        var reservedUnits = activeOrders
            .Where(o => OrderOverlapsRange(o, effectiveStartUtc, effectiveEndUtc, timeZone))
            .Sum(o => o.UnitsRequested <= 0 ? 1 : o.UnitsRequested);

        if (reservedUnits + unitsRequested > rental.UnitsAvailable)
            return "Not enough units available for the selected dates.";

        return null;
    }

    private static string? NormalizeText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private static DateTime? NormalizeUtc(DateTime? value)
    {
        if (!value.HasValue)
            return null;

        return value.Value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
    }

    private static string NormalizeCurrency(string? currency)
    {
        return string.IsNullOrWhiteSpace(currency)
            ? "USD"
            : currency.Trim().ToUpperInvariant();
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

    private static bool OrderOverlapsRange(
        RentalOrder order,
        DateTime rangeStartUtc,
        DateTime rangeEndUtc,
        TimeZoneInfo timeZone)
    {
        if (!order.StartUtc.HasValue || !order.EndUtc.HasValue)
            return true;

        var effectiveEndUtc = ResolveEffectiveEndUtc(order, timeZone);
        return order.StartUtc.Value < rangeEndUtc && effectiveEndUtc > rangeStartUtc;
    }

    private static DateTime ResolveEffectiveEndUtc(
        RentalPricingUnit pricingUnit,
        DateTime startUtc,
        DateTime endUtc,
        TimeZoneInfo timeZone)
    {
        var localStart = ConvertUtcToLocal(startUtc, timeZone);

        return pricingUnit switch
        {
            RentalPricingUnit.Week => ConvertLocalToUtc(GetStartOfNextWeekLocal(localStart), timeZone),
            RentalPricingUnit.Month => ConvertLocalToUtc(GetStartOfNextMonthLocal(localStart), timeZone),
            RentalPricingUnit.Year => ConvertLocalToUtc(GetStartOfNextYearLocal(localStart), timeZone),
            _ => endUtc
        };
    }

    private static DateTime ResolveEffectiveEndUtc(RentalOrder order, TimeZoneInfo timeZone)
    {
        if (!order.StartUtc.HasValue || !order.EndUtc.HasValue)
            return order.EndUtc ?? DateTime.MaxValue;

        return ResolveEffectiveEndUtc(order.PricingUnit, order.StartUtc.Value, order.EndUtc.Value, timeZone);
    }

    private static DateTime ConvertLocalToUtc(DateTime localTime, TimeZoneInfo timeZone)
    {
        var unspecified = DateTime.SpecifyKind(localTime, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecified, timeZone);
    }

    private static DateTime ConvertUtcToLocal(DateTime utcTime, TimeZoneInfo timeZone)
    {
        var utc = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, timeZone);
    }

    private static DateTime GetStartOfNextMonthLocal(DateTime localStart)
        => new DateTime(localStart.Year, localStart.Month, 1, 0, 0, 0).AddMonths(1);

    private static DateTime GetStartOfNextYearLocal(DateTime localStart)
        => new DateTime(localStart.Year, 1, 1, 0, 0, 0).AddYears(1);

    private static DateTime GetStartOfNextWeekLocal(DateTime localStart)
    {
        var daysUntilNextSunday = localStart.DayOfWeek == DayOfWeek.Sunday
            ? 7
            : 7 - (int)localStart.DayOfWeek;
        return localStart.Date.AddDays(daysUntilNextSunday);
    }

    private bool IsTenantActor(Guid tenantId)
        => userContext.TenantId.HasValue && userContext.TenantId.Value == tenantId;

    private bool IsCustomerActor(RentalOrder order)
        => userContext.UserId.HasValue && order.FkCustomerId == userContext.UserId.Value;

    private static bool ValidateStatusTransition(RentalOrderStatus current, RentalOrderStatus next)
    {
        if (current is RentalOrderStatus.Completed or RentalOrderStatus.Declined or RentalOrderStatus.Cancelled)
            return false;

        return current switch
        {
            RentalOrderStatus.Pending => next is RentalOrderStatus.Confirmed or RentalOrderStatus.Declined or RentalOrderStatus.Cancelled,
            RentalOrderStatus.Confirmed => next is RentalOrderStatus.InProgress or RentalOrderStatus.Completed or RentalOrderStatus.Cancelled,
            RentalOrderStatus.InProgress => next is RentalOrderStatus.Completed,
            _ => false
        };
    }

    private static IQueryable<RentalOrder> ApplyRangeFilter(
        IQueryable<RentalOrder> query,
        DateTime? fromUtc,
        DateTime? toUtc)
    {
        if (fromUtc.HasValue)
            query = query.Where(o => o.EndUtc == null || o.EndUtc > fromUtc.Value);

        if (toUtc.HasValue)
            query = query.Where(o => o.StartUtc == null || o.StartUtc < toUtc.Value);

        return query;
    }

    private static IQueryable<VwRentalOrderListItem> ApplySorting(
        IQueryable<VwRentalOrderListItem> query,
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
            "rental" or "rentaltitle" => descending ? query.OrderByDescending(x => x.RentalTitle) : query.OrderBy(x => x.RentalTitle),
            "tenant" or "tenantname" => descending ? query.OrderByDescending(x => x.TenantName) : query.OrderBy(x => x.TenantName),
            _ => descending ? query.OrderByDescending(x => x.DateCreated) : query.OrderBy(x => x.DateCreated)
        };
    }

    private static string? ValidateRequest(RentalOrderCreateRequest request)
    {
        if (request.RentalId == Guid.Empty)
            return "RentalId is required.";

        if (request.StartUtc == null || request.EndUtc == null)
            return "StartUtc and EndUtc are required for rentals.";

        return null;
    }

    private async Task<bool> TenantSupportsRentalsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var code = BookingCategoryTypeMapper.ToCode(BookingCategoryType.Spaces);
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
}


