using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Response;
using Bookazone.Application.DTOs.Converter;
using Bookazone.Application.DTOs.Equipment;
using Bookazone.Application.Interfaces.Context;
using Bookazone.Application.Interfaces.Repository.Booking;
using Bookazone.Application.Interfaces.Repository.Equipment;
using Bookazone.Application.Interfaces.Repository.Subscription;
using Bookazone.Application.Interfaces.Services.Equipment;
using Bookazone.Domain.Entities.Equipment;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Services.Equipment;

public class EquipmentOrderService(
    IEquipmentOrderRepository equipmentOrderRepository,
    IEquipmentRepository equipmentRepository,
    IEquipmentPricingRuleRepository equipmentPricingRuleRepository,
    ITenantBookingCategoryRepository tenantBookingCategoryRepository,
    ITenantSettingsRepository tenantSettingsRepository,
    IUserContext userContext,
    IHttpContextAccessor httpContextAccessor,
    ILogger<EquipmentOrderService> logger)
    : IEquipmentOrderService
{
    public async Task<ApiResult> CreateAsync(EquipmentOrderCreateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!userContext.UserId.HasValue)
                return ApiResponse.Error(ErrorHttp.Forbidden,
                    new Exception("User context does not contain a valid user id."));

            var equipment = await equipmentRepository.Query()
                .AsNoTracking()
                .Include(e => e.FkCoverMedia)
                .ThenInclude(m => m.FkStoredFile)
                .FirstOrDefaultAsync(e =>
                        e.Id == request.EquipmentId &&
                        e.Active &&
                        !e.Deleted &&
                        e.Status == EquipmentStatus.Published,
                    cancellationToken);

            if (equipment == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!await TenantSupportsEquipmentAsync(equipment.FkTenantId, cancellationToken))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Tenant is not enabled for equipment rentals.");

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

            var availabilityError = await ValidateAvailabilityAsync(
                equipment,
                startUtc,
                endUtc,
                request.PricingUnit,
                unitsRequested,
                cancellationToken);
            if (!string.IsNullOrWhiteSpace(availabilityError))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, availabilityError);

            var pricingRule = await ResolvePricingRuleAsync(equipment.Id, request.PricingUnit, quantity, cancellationToken);
            if (pricingRule == null)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Pricing rule not configured for this unit/quantity.");

            var unitPrice = pricingRule.PriceAmount;
            var totalPrice = Math.Round(unitPrice * quantity * unitsRequested, 2, MidpointRounding.AwayFromZero);
            var currency = NormalizeCurrency(pricingRule.Currency);
            var status = equipment.BookingMode == BookingMode.Instant
                ? EquipmentOrderStatus.Confirmed
                : EquipmentOrderStatus.Pending;

            var order = new EquipmentOrder
            {
                FkEquipmentId = equipment.Id,
                FkTenantId = equipment.FkTenantId,
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
                DeliveryAddress = NormalizeText(request.DeliveryAddress, 300),
                DeliveryLatitude = request.DeliveryLatitude,
                DeliveryLongitude = request.DeliveryLongitude,
                ContactName = NormalizeText(request.ContactName, 150),
                ContactEmail = NormalizeText(request.ContactEmail, 200),
                ContactPhone = NormalizeText(request.ContactPhone, 50),
                Notes = NormalizeText(request.Notes, 2000),
                Active = true,
                Deleted = false
            };

            var created = await equipmentOrderRepository.AddAsync(order);
            created.FkEquipment = equipment;
            return ApiResponse.Success(ToDetail(created, equipment, null));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating equipment order");
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

            return ApiResponse.Success(ToDetail(order, order.FkEquipment, order.FkTenant));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting equipment order {OrderId}", orderId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetCustomerOrdersAsync(
        List<EquipmentOrderStatus>? statuses = null,
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

            var query = equipmentOrderRepository.Query()
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

            var projected = query.Select(o => new VwEquipmentOrderListItem
            {
                Id = o.Id,
                EquipmentId = o.FkEquipmentId,
                EquipmentTitle = o.FkEquipment.Title,
                EquipmentCategory = o.FkEquipment.Category,
                EquipmentCoverUrl = o.FkEquipment.FkCoverMedia != null
                    ? o.FkEquipment.FkCoverMedia.FkStoredFile.RelativePath
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
            var paged = await Paginate<VwEquipmentOrderListItem>.CreateAsync(sorted, pageIndex, pageSize);

            var request = httpContextAccessor.HttpContext?.Request;
            foreach (var item in paged.Data)
                item.EquipmentCoverUrl = FileUrlHelper.BuildPublicUrl(request, item.EquipmentCoverUrl);

            return ApiResponse.Success(paged);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting customer equipment orders");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetTenantOrdersAsync(
        List<EquipmentOrderStatus>? statuses = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        Guid? equipmentId = null,
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

            var query = equipmentOrderRepository.Query()
                .AsNoTracking()
                .Where(o =>
                    o.FkTenantId == tenantId &&
                    o.Active &&
                    !o.Deleted);

            if (equipmentId.HasValue)
                query = query.Where(o => o.FkEquipmentId == equipmentId.Value);

            if (customerId.HasValue)
                query = query.Where(o => o.FkCustomerId == customerId.Value);

            query = ApplyRangeFilter(query, fromUtc, toUtc);

            if (statuses is { Count: > 0 })
                query = query.Where(o => statuses.Contains(o.Status));

            var projected = query.Select(o => new VwEquipmentOrderListItem
            {
                Id = o.Id,
                EquipmentId = o.FkEquipmentId,
                EquipmentTitle = o.FkEquipment.Title,
                EquipmentCategory = o.FkEquipment.Category,
                EquipmentCoverUrl = o.FkEquipment.FkCoverMedia != null
                    ? o.FkEquipment.FkCoverMedia.FkStoredFile.RelativePath
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
            var paged = await Paginate<VwEquipmentOrderListItem>.CreateAsync(sorted, pageIndex, pageSize);

            var request = httpContextAccessor.HttpContext?.Request;
            foreach (var item in paged.Data)
                item.EquipmentCoverUrl = FileUrlHelper.BuildPublicUrl(request, item.EquipmentCoverUrl);

            return ApiResponse.Success(paged);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant equipment orders");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpdateStatusAsync(EquipmentOrderStatusUpdateRequest request, CancellationToken cancellationToken = default)
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
            var updated = await equipmentOrderRepository.UpdateAsync(order);
            return ApiResponse.Success(await ToDetailAsync(updated, cancellationToken));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating equipment order status {OrderId}", request.OrderId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> CancelAsync(EquipmentOrderCancelRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await GetOrderForUpdateAsync(request.OrderId, cancellationToken);
            if (order == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!IsTenantActor(order.FkTenantId) && !IsCustomerActor(order))
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User is not authorized to cancel this order."));

            if (order.Status is EquipmentOrderStatus.Cancelled or EquipmentOrderStatus.Declined or EquipmentOrderStatus.Completed)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Order cannot be cancelled in its current status.");

            order.Status = EquipmentOrderStatus.Cancelled;
            var updated = await equipmentOrderRepository.UpdateAsync(order);
            return ApiResponse.Success(await ToDetailAsync(updated, cancellationToken));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cancelling equipment order {OrderId}", request.OrderId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    private async Task<EquipmentOrder?> LoadOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await equipmentOrderRepository.Query()
            .AsNoTracking()
            .Include(o => o.FkEquipment)
            .ThenInclude(e => e.FkCoverMedia)
            .ThenInclude(m => m.FkStoredFile)
            .Include(o => o.FkTenant)
            .Include(o => o.FkCustomer)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.Active && !o.Deleted, cancellationToken);
    }

    private Task<EquipmentOrder?> GetOrderForUpdateAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return equipmentOrderRepository.Query()
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.Deleted, cancellationToken);
    }

    private async Task<VwEquipmentOrderDetail> ToDetailAsync(EquipmentOrder order, CancellationToken cancellationToken)
    {
        var loaded = await LoadOrderAsync(order.Id, cancellationToken) ?? order;
        return ToDetail(loaded, loaded.FkEquipment, loaded.FkTenant);
    }

    private VwEquipmentOrderDetail ToDetail(EquipmentOrder order, Bookazone.Domain.Entities.Equipment.Equipment? equipment, Domain.Entities.Tenant.Tenants? tenant)
    {
        var coverUrl = equipment?.FkCoverMedia?.FkStoredFile?.RelativePath;
        var request = httpContextAccessor.HttpContext?.Request;
        coverUrl = FileUrlHelper.BuildPublicUrl(request, coverUrl);

        var customerName = ((order.FkCustomer?.Firstname ?? "") + " " + (order.FkCustomer?.Lastname ?? "")).Trim();
        return EquipmentOrderConverter.ToDetail(
            order,
            coverUrl,
            customerName,
            order.FkCustomer?.Email,
            tenant?.Name,
            equipment?.Title,
            equipment?.Category);
    }

    private async Task<EquipmentPricingRule?> ResolvePricingRuleAsync(
        Guid equipmentId,
        EquipmentPricingUnit unit,
        decimal quantity,
        CancellationToken cancellationToken)
    {
        var rules = await equipmentPricingRuleRepository.GetByEquipmentAsync(equipmentId, cancellationToken);
        var applicable = rules
            .Where(r => r.Unit == unit && IsQuantityInRange(r, quantity))
            .ToList();

        if (applicable.Count == 0)
            return null;

        return applicable.FirstOrDefault(r => r.IsPrimary)
               ?? applicable.OrderBy(r => r.PriceAmount).First();
    }

    private static bool IsQuantityInRange(EquipmentPricingRule rule, decimal quantity)
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
        EquipmentPricingUnit unit,
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
            error = "StartUtc and EndUtc are required for equipment rentals.";
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
            EquipmentPricingUnit.Hour => (decimal)duration.TotalHours,
            EquipmentPricingUnit.Day => (decimal)duration.TotalDays,
            EquipmentPricingUnit.Week => (decimal)(duration.TotalDays / 7d),
            EquipmentPricingUnit.Month => (decimal)(duration.TotalDays / 30d),
            EquipmentPricingUnit.Year => (decimal)(duration.TotalDays / 365d),
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
        Bookazone.Domain.Entities.Equipment.Equipment equipment,
        DateTime? startUtc,
        DateTime? endUtc,
        EquipmentPricingUnit pricingUnit,
        int unitsRequested,
        CancellationToken cancellationToken)
    {
        if (!startUtc.HasValue || !endUtc.HasValue)
            return "StartUtc and EndUtc are required for equipment rentals.";

        if (unitsRequested <= 0)
            return "UnitsRequested must be greater than zero.";

        if (equipment.UnitsAvailable <= 0)
            return "No units available for this equipment.";

        if (unitsRequested > equipment.UnitsAvailable)
            return "Requested units exceed available inventory.";

        var timeZone = await ResolveTenantTimeZoneAsync(equipment.FkTenantId);
        var effectiveStartUtc = startUtc.Value;
        var effectiveEndUtc = ResolveEffectiveEndUtc(pricingUnit, startUtc.Value, endUtc.Value, timeZone);

        var activeOrders = await equipmentOrderRepository.Query()
            .AsNoTracking()
            .Where(o =>
                o.FkEquipmentId == equipment.Id &&
                o.Active &&
                !o.Deleted &&
                o.Status != EquipmentOrderStatus.Cancelled &&
                o.Status != EquipmentOrderStatus.Declined &&
                o.Status != EquipmentOrderStatus.Completed &&
                (o.StartUtc == null ||
                 o.EndUtc == null ||
                 (o.StartUtc < effectiveEndUtc && o.EndUtc > effectiveStartUtc) ||
                 ((o.PricingUnit == EquipmentPricingUnit.Week ||
                   o.PricingUnit == EquipmentPricingUnit.Month ||
                   o.PricingUnit == EquipmentPricingUnit.Year) &&
                  o.StartUtc < effectiveEndUtc)))
            .ToListAsync(cancellationToken);

        var reservedUnits = activeOrders
            .Where(o => OrderOverlapsRange(o, effectiveStartUtc, effectiveEndUtc, timeZone))
            .Sum(o => o.UnitsRequested <= 0 ? 1 : o.UnitsRequested);

        if (reservedUnits + unitsRequested > equipment.UnitsAvailable)
            return "Not enough units available for the selected dates.";

        return null;
    }

    private static string? ValidateRequest(EquipmentOrderCreateRequest request)
    {
        if (request.EquipmentId == Guid.Empty)
            return "EquipmentId is required.";

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
        EquipmentOrder order,
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
        EquipmentPricingUnit pricingUnit,
        DateTime startUtc,
        DateTime endUtc,
        TimeZoneInfo timeZone)
    {
        var localStart = ConvertUtcToLocal(startUtc, timeZone);

        return pricingUnit switch
        {
            EquipmentPricingUnit.Week => ConvertLocalToUtc(GetStartOfNextWeekLocal(localStart), timeZone),
            EquipmentPricingUnit.Month => ConvertLocalToUtc(GetStartOfNextMonthLocal(localStart), timeZone),
            EquipmentPricingUnit.Year => ConvertLocalToUtc(GetStartOfNextYearLocal(localStart), timeZone),
            _ => endUtc
        };
    }

    private static DateTime ResolveEffectiveEndUtc(EquipmentOrder order, TimeZoneInfo timeZone)
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

    private bool IsCustomerActor(EquipmentOrder order)
        => userContext.UserId.HasValue && order.FkCustomerId == userContext.UserId.Value;

    private static bool ValidateStatusTransition(EquipmentOrderStatus current, EquipmentOrderStatus next)
    {
        if (current is EquipmentOrderStatus.Completed or EquipmentOrderStatus.Declined or EquipmentOrderStatus.Cancelled)
            return false;

        return current switch
        {
            EquipmentOrderStatus.Pending => next is EquipmentOrderStatus.Confirmed or EquipmentOrderStatus.Declined or EquipmentOrderStatus.Cancelled,
            EquipmentOrderStatus.Confirmed => next is EquipmentOrderStatus.InProgress or EquipmentOrderStatus.Completed or EquipmentOrderStatus.Cancelled,
            EquipmentOrderStatus.InProgress => next is EquipmentOrderStatus.Completed,
            _ => false
        };
    }

    private static IQueryable<EquipmentOrder> ApplyRangeFilter(
        IQueryable<EquipmentOrder> query,
        DateTime? fromUtc,
        DateTime? toUtc)
    {
        if (fromUtc.HasValue)
            query = query.Where(o => o.EndUtc == null || o.EndUtc > fromUtc.Value);

        if (toUtc.HasValue)
            query = query.Where(o => o.StartUtc == null || o.StartUtc < toUtc.Value);

        return query;
    }

    private static IQueryable<VwEquipmentOrderListItem> ApplySorting(
        IQueryable<VwEquipmentOrderListItem> query,
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
            "equipment" or "equipmenttitle" => descending ? query.OrderByDescending(x => x.EquipmentTitle) : query.OrderBy(x => x.EquipmentTitle),
            "tenant" or "tenantname" => descending ? query.OrderByDescending(x => x.TenantName) : query.OrderBy(x => x.TenantName),
            _ => descending ? query.OrderByDescending(x => x.DateCreated) : query.OrderBy(x => x.DateCreated)
        };
    }

    private async Task<bool> TenantSupportsEquipmentAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var code = BookingCategoryTypeMapper.ToCode(BookingCategoryType.EquipmentRental);
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
