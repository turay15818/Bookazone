using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Response;
using Bookazone.Application.DTOs.Converter;
using Bookazone.Application.DTOs.Vehicles;
using Bookazone.Application.Interfaces.Context;
using Bookazone.Application.Interfaces.Repository.Booking;
using Bookazone.Application.Interfaces.Repository.Vehicles;
using Bookazone.Application.Interfaces.Services.Vehicles;
using Bookazone.Domain.Entities.Vehicles;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Services.Vehicles;

public class VehicleOrderService(
    IVehicleOrderRepository vehicleOrderRepository,
    IVehicleRepository vehicleRepository,
    IVehiclePricingRuleRepository vehiclePricingRuleRepository,
    ITenantBookingCategoryRepository tenantBookingCategoryRepository,
    IUserContext userContext,
    IHttpContextAccessor httpContextAccessor,
    ILogger<VehicleOrderService> logger)
    : IVehicleOrderService
{
    public async Task<ApiResult> CreateAsync(VehicleOrderCreateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!userContext.UserId.HasValue)
                return ApiResponse.Error(ErrorHttp.Forbidden,
                    new Exception("User context does not contain a valid user id."));

            var vehicle = await vehicleRepository.Query()
                .AsNoTracking()
                .Include(v => v.FkCoverMedia)
                .ThenInclude(m => m.FkStoredFile)
                .FirstOrDefaultAsync(v =>
                        v.Id == request.VehicleId &&
                        v.Active &&
                        !v.Deleted &&
                        v.Status == VehicleStatus.Published,
                    cancellationToken);

            if (vehicle == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!await TenantSupportsVehiclesAsync(vehicle.FkTenantId, cancellationToken))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Tenant is not enabled for vehicles.");

            var validationError = ValidateRequest(vehicle.ServiceType, request);
            if (!string.IsNullOrWhiteSpace(validationError))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validationError);

            var startUtc = NormalizeUtc(request.StartUtc);
            var endUtc = NormalizeUtc(request.EndUtc);

            if (!TryResolveQuantity(
                    request.PricingUnit,
                    request.Quantity,
                    startUtc,
                    endUtc,
                    request.CargoWeightTons,
                    out var quantity,
                    out var quantityError))
            {
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, quantityError ?? "Invalid quantity.");
            }

            var pricingRule = await ResolvePricingRuleAsync(vehicle.Id, request.PricingUnit, quantity, cancellationToken);
            if (pricingRule == null)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Pricing rule not configured for this unit/quantity.");

            var driverRequested = request.DriverRequested ?? vehicle.DriverOption == VehicleDriverOption.Included;
            if (vehicle.DriverOption == VehicleDriverOption.NotAvailable && driverRequested)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Driver option is not available for this vehicle.");

            var unitPrice = pricingRule.PriceAmount;
            var totalPrice = Math.Round(unitPrice * quantity, 2, MidpointRounding.AwayFromZero);
            var currency = NormalizeCurrency(pricingRule.Currency);
            var status = vehicle.BookingMode == BookingMode.Instant
                ? VehicleOrderStatus.Confirmed
                : VehicleOrderStatus.Pending;

            var order = new VehicleOrder
            {
                FkVehicleId = vehicle.Id,
                FkTenantId = vehicle.FkTenantId,
                FkCustomerId = userContext.UserId.Value,
                ServiceType = vehicle.ServiceType,
                Status = status,
                PricingUnit = request.PricingUnit,
                Quantity = quantity,
                UnitPrice = unitPrice,
                TotalPrice = totalPrice,
                Currency = currency,
                StartUtc = startUtc,
                EndUtc = endUtc,
                PickupAddress = NormalizeText(request.PickupAddress, 300),
                DropoffAddress = NormalizeText(request.DropoffAddress, 300),
                PickupLatitude = request.PickupLatitude,
                PickupLongitude = request.PickupLongitude,
                DropoffLatitude = request.DropoffLatitude,
                DropoffLongitude = request.DropoffLongitude,
                CargoDescription = NormalizeText(request.CargoDescription, 2000),
                CargoWeightTons = request.CargoWeightTons,
                CargoVolumeCubicMeters = request.CargoVolumeCubicMeters,
                DriverRequested = driverRequested,
                ContactName = NormalizeText(request.ContactName, 150),
                ContactEmail = NormalizeText(request.ContactEmail, 200),
                ContactPhone = NormalizeText(request.ContactPhone, 50),
                Notes = NormalizeText(request.Notes, 2000),
                Active = true,
                Deleted = false
            };

            var created = await vehicleOrderRepository.AddAsync(order);
            created.FkVehicle = vehicle;
            return ApiResponse.Success(ToDetail(created, vehicle, null));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating vehicle order");
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

            return ApiResponse.Success(ToDetail(order, order.FkVehicle, order.FkTenant));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting vehicle order {OrderId}", orderId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetCustomerOrdersAsync(
        List<VehicleOrderStatus>? statuses = null,
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

            var query = vehicleOrderRepository.Query()
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

            var projected = query.Select(o => new VwVehicleOrderListItem
            {
                Id = o.Id,
                VehicleId = o.FkVehicleId,
                VehicleTitle = o.FkVehicle.Title,
                VehicleType = o.FkVehicle.Type,
                ServiceType = o.ServiceType,
                VehicleCoverUrl = o.FkVehicle.FkCoverMedia != null
                    ? o.FkVehicle.FkCoverMedia.FkStoredFile.RelativePath
                    : null,
                TenantId = o.FkTenantId,
                TenantName = o.FkTenant.Name,
                CustomerId = o.FkCustomerId,
                CustomerName = ((o.FkCustomer.Firstname ?? "") + " " + (o.FkCustomer.Lastname ?? "")).Trim(),
                CustomerEmail = o.FkCustomer.Email,
                Status = o.Status,
                PricingUnit = o.PricingUnit,
                Quantity = o.Quantity,
                UnitPrice = o.UnitPrice,
                TotalPrice = o.TotalPrice,
                Currency = o.Currency,
                StartUtc = o.StartUtc,
                EndUtc = o.EndUtc,
                DateCreated = o.DateCreated
            });

            var sorted = ApplySorting(projected, sortBy, sortDirection);
            var paged = await Paginate<VwVehicleOrderListItem>.CreateAsync(sorted, pageIndex, pageSize);

            var request = httpContextAccessor.HttpContext?.Request;
            foreach (var item in paged.Data)
                item.VehicleCoverUrl = FileUrlHelper.BuildPublicUrl(request, item.VehicleCoverUrl);

            return ApiResponse.Success(paged);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting customer vehicle orders");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetTenantOrdersAsync(
        List<VehicleOrderStatus>? statuses = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        Guid? vehicleId = null,
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

            var query = vehicleOrderRepository.Query()
                .AsNoTracking()
                .Where(o =>
                    o.FkTenantId == tenantId &&
                    o.Active &&
                    !o.Deleted);

            if (vehicleId.HasValue)
                query = query.Where(o => o.FkVehicleId == vehicleId.Value);

            if (customerId.HasValue)
                query = query.Where(o => o.FkCustomerId == customerId.Value);

            query = ApplyRangeFilter(query, fromUtc, toUtc);

            if (statuses is { Count: > 0 })
                query = query.Where(o => statuses.Contains(o.Status));

            var projected = query.Select(o => new VwVehicleOrderListItem
            {
                Id = o.Id,
                VehicleId = o.FkVehicleId,
                VehicleTitle = o.FkVehicle.Title,
                VehicleType = o.FkVehicle.Type,
                ServiceType = o.ServiceType,
                VehicleCoverUrl = o.FkVehicle.FkCoverMedia != null
                    ? o.FkVehicle.FkCoverMedia.FkStoredFile.RelativePath
                    : null,
                TenantId = o.FkTenantId,
                TenantName = o.FkTenant.Name,
                CustomerId = o.FkCustomerId,
                CustomerName = ((o.FkCustomer.Firstname ?? "") + " " + (o.FkCustomer.Lastname ?? "")).Trim(),
                CustomerEmail = o.FkCustomer.Email,
                Status = o.Status,
                PricingUnit = o.PricingUnit,
                Quantity = o.Quantity,
                UnitPrice = o.UnitPrice,
                TotalPrice = o.TotalPrice,
                Currency = o.Currency,
                StartUtc = o.StartUtc,
                EndUtc = o.EndUtc,
                DateCreated = o.DateCreated
            });

            var sorted = ApplySorting(projected, sortBy, sortDirection);
            var paged = await Paginate<VwVehicleOrderListItem>.CreateAsync(sorted, pageIndex, pageSize);

            var request = httpContextAccessor.HttpContext?.Request;
            foreach (var item in paged.Data)
                item.VehicleCoverUrl = FileUrlHelper.BuildPublicUrl(request, item.VehicleCoverUrl);

            return ApiResponse.Success(paged);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant vehicle orders");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpdateStatusAsync(VehicleOrderStatusUpdateRequest request, CancellationToken cancellationToken = default)
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
            var updated = await vehicleOrderRepository.UpdateAsync(order);
            return ApiResponse.Success(await ToDetailAsync(updated, cancellationToken));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating vehicle order status {OrderId}", request.OrderId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> CancelAsync(VehicleOrderCancelRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await GetOrderForUpdateAsync(request.OrderId, cancellationToken);
            if (order == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!IsTenantActor(order.FkTenantId) && !IsCustomerActor(order))
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User is not authorized to cancel this order."));

            if (order.Status is VehicleOrderStatus.Cancelled or VehicleOrderStatus.Declined or VehicleOrderStatus.Completed)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Order cannot be cancelled in its current status.");

            order.Status = VehicleOrderStatus.Cancelled;
            var updated = await vehicleOrderRepository.UpdateAsync(order);
            return ApiResponse.Success(await ToDetailAsync(updated, cancellationToken));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cancelling vehicle order {OrderId}", request.OrderId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    private async Task<VehicleOrder?> LoadOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await vehicleOrderRepository.Query()
            .AsNoTracking()
            .Include(o => o.FkVehicle)
            .ThenInclude(v => v.FkCoverMedia)
            .ThenInclude(m => m.FkStoredFile)
            .Include(o => o.FkTenant)
            .Include(o => o.FkCustomer)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.Active && !o.Deleted, cancellationToken);
    }

    private Task<VehicleOrder?> GetOrderForUpdateAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return vehicleOrderRepository.Query()
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.Deleted, cancellationToken);
    }

    private async Task<VwVehicleOrderDetail> ToDetailAsync(VehicleOrder order, CancellationToken cancellationToken)
    {
        var loaded = await LoadOrderAsync(order.Id, cancellationToken) ?? order;
        return ToDetail(loaded, loaded.FkVehicle, loaded.FkTenant);
    }

    private VwVehicleOrderDetail ToDetail(VehicleOrder order, Vehicle? vehicle, Domain.Entities.Tenant.Tenants? tenant)
    {
        var coverUrl = vehicle?.FkCoverMedia?.FkStoredFile?.RelativePath;
        var request = httpContextAccessor.HttpContext?.Request;
        coverUrl = FileUrlHelper.BuildPublicUrl(request, coverUrl);

        var customerName = ((order.FkCustomer?.Firstname ?? "") + " " + (order.FkCustomer?.Lastname ?? "")).Trim();
        return VehicleOrderConverter.ToDetail(
            order,
            coverUrl,
            customerName,
            order.FkCustomer?.Email,
            tenant?.Name,
            vehicle?.Title);
    }

    private async Task<VehiclePricingRule?> ResolvePricingRuleAsync(
        Guid vehicleId,
        VehiclePricingUnit unit,
        decimal quantity,
        CancellationToken cancellationToken)
    {
        var rules = await vehiclePricingRuleRepository.GetByVehicleAsync(vehicleId, cancellationToken);
        var applicable = rules
            .Where(r => r.Unit == unit && IsQuantityInRange(r, quantity))
            .ToList();

        if (applicable.Count == 0)
            return null;

        return applicable.FirstOrDefault(r => r.IsPrimary)
               ?? applicable.OrderBy(r => r.PriceAmount).First();
    }

    private static bool IsQuantityInRange(VehiclePricingRule rule, decimal quantity)
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
        VehiclePricingUnit unit,
        decimal? requestedQuantity,
        DateTime? startUtc,
        DateTime? endUtc,
        decimal? cargoWeightTons,
        out decimal quantity,
        out string? error)
    {
        quantity = 0m;
        error = null;

        if (unit is VehiclePricingUnit.Hour or VehiclePricingUnit.Day)
        {
            if (!startUtc.HasValue || !endUtc.HasValue)
            {
                error = "StartUtc and EndUtc are required for hourly or daily pricing.";
                return false;
            }

            if (endUtc.Value <= startUtc.Value)
            {
                error = "EndUtc must be greater than StartUtc.";
                return false;
            }

            var duration = endUtc.Value - startUtc.Value;
            quantity = unit == VehiclePricingUnit.Hour
                ? (decimal)duration.TotalHours
                : (decimal)duration.TotalDays;

            if (requestedQuantity.HasValue && requestedQuantity.Value > 0)
                quantity = requestedQuantity.Value;

            if (quantity <= 0)
            {
                error = "Quantity must be greater than zero.";
                return false;
            }

            return true;
        }

        if (unit == VehiclePricingUnit.Ton)
        {
            quantity = requestedQuantity.HasValue && requestedQuantity.Value > 0
                ? requestedQuantity.Value
                : cargoWeightTons ?? 0m;

            if (quantity <= 0)
            {
                error = "Quantity must be greater than zero.";
                return false;
            }

            return true;
        }

        if (unit == VehiclePricingUnit.Trip)
        {
            quantity = requestedQuantity.HasValue && requestedQuantity.Value > 0 ? requestedQuantity.Value : 1m;
            return true;
        }

        quantity = requestedQuantity ?? 0m;
        if (quantity <= 0)
        {
            error = "Quantity must be greater than zero.";
            return false;
        }

        return true;
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

    private bool IsTenantActor(Guid tenantId)
        => userContext.TenantId.HasValue && userContext.TenantId.Value == tenantId;

    private bool IsCustomerActor(VehicleOrder order)
        => userContext.UserId.HasValue && order.FkCustomerId == userContext.UserId.Value;

    private static bool ValidateStatusTransition(VehicleOrderStatus current, VehicleOrderStatus next)
    {
        if (current is VehicleOrderStatus.Completed or VehicleOrderStatus.Declined or VehicleOrderStatus.Cancelled)
            return false;

        return current switch
        {
            VehicleOrderStatus.Pending => next is VehicleOrderStatus.Confirmed or VehicleOrderStatus.Declined or VehicleOrderStatus.Cancelled,
            VehicleOrderStatus.Confirmed => next is VehicleOrderStatus.InProgress or VehicleOrderStatus.Completed or VehicleOrderStatus.Cancelled,
            VehicleOrderStatus.InProgress => next is VehicleOrderStatus.Completed,
            _ => false
        };
    }

    private static IQueryable<VehicleOrder> ApplyRangeFilter(
        IQueryable<VehicleOrder> query,
        DateTime? fromUtc,
        DateTime? toUtc)
    {
        if (fromUtc.HasValue)
            query = query.Where(o => o.EndUtc == null || o.EndUtc > fromUtc.Value);

        if (toUtc.HasValue)
            query = query.Where(o => o.StartUtc == null || o.StartUtc < toUtc.Value);

        return query;
    }

    private static IQueryable<VwVehicleOrderListItem> ApplySorting(
        IQueryable<VwVehicleOrderListItem> query,
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
            "vehicle" or "vehicletitle" => descending ? query.OrderByDescending(x => x.VehicleTitle) : query.OrderBy(x => x.VehicleTitle),
            "tenant" or "tenantname" => descending ? query.OrderByDescending(x => x.TenantName) : query.OrderBy(x => x.TenantName),
            _ => descending ? query.OrderByDescending(x => x.DateCreated) : query.OrderBy(x => x.DateCreated)
        };
    }

    private static string? ValidateRequest(VehicleServiceType serviceType, VehicleOrderCreateRequest request)
    {
        if (request.VehicleId == Guid.Empty)
            return "VehicleId is required.";

        if (serviceType == VehicleServiceType.GoodsTransport)
        {
            if (string.IsNullOrWhiteSpace(request.PickupAddress))
                return "PickupAddress is required for goods transport.";
            if (string.IsNullOrWhiteSpace(request.DropoffAddress))
                return "DropoffAddress is required for goods transport.";
        }

        if (serviceType == VehicleServiceType.Rental &&
            (request.StartUtc == null || request.EndUtc == null))
            return "StartUtc and EndUtc are required for rentals.";

        return null;
    }

    private async Task<bool> TenantSupportsVehiclesAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var code = BookingCategoryTypeMapper.ToCode(BookingCategoryType.Vehicles);
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
