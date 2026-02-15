using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Converter;
using Bookazone.Application.DTOs.Equipment;
using Bookazone.Application.DTOs.Response;
using Bookazone.Application.Interfaces.Repository.Equipment;
using Bookazone.Application.Interfaces.Repository.Subscription;
using Bookazone.Application.Interfaces.Services.Equipment;
using Bookazone.Domain.Entities.Equipment;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Services.Equipment;

public class EquipmentDiscoveryService(
    IEquipmentRepository equipmentRepository,
    IEquipmentPricingRuleRepository equipmentPricingRuleRepository,
    IEquipmentSpecRepository equipmentSpecRepository,
    IEquipmentPolicyRepository equipmentPolicyRepository,
    IEquipmentMediaRepository equipmentMediaRepository,
    IEquipmentOrderRepository equipmentOrderRepository,
    ITenantSettingsRepository tenantSettingsRepository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<EquipmentDiscoveryService> logger)
    : IEquipmentDiscoveryService
{
    public async Task<ApiResult> GetEquipmentsAsync(EquipmentSearchRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            request ??= new EquipmentSearchRequest();

            var query = equipmentRepository.Query()
                .AsNoTracking()
                .Include(e => e.FkTenant)
                .Include(e => e.FkCoverMedia)
                .ThenInclude(m => m.FkStoredFile)
                .Where(e =>
                    e.Active &&
                    !e.Deleted &&
                    e.Status == EquipmentStatus.Published);

            if (request.TenantId.HasValue)
                query = query.Where(e => e.FkTenantId == request.TenantId.Value);

            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                var category = request.Category.Trim().ToLowerInvariant();
                query = query.Where(e => e.Category != null && e.Category.ToLower().Contains(category));
            }

            if (!string.IsNullOrWhiteSpace(request.City))
            {
                var city = request.City.Trim().ToLowerInvariant();
                query = query.Where(e => e.City != null && e.City.ToLower().Contains(city));
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = request.Search.Trim().ToLowerInvariant();
                query = query.Where(e =>
                    e.Title.ToLower().Contains(term) ||
                    (e.Subtitle != null && e.Subtitle.ToLower().Contains(term)) ||
                    (e.Description != null && e.Description.ToLower().Contains(term)) ||
                    (e.Category != null && e.Category.ToLower().Contains(term)) ||
                    (e.City != null && e.City.ToLower().Contains(term)) ||
                    e.FkTenant.Name.ToLower().Contains(term));
            }

            if (request.MinUnitsAvailable.HasValue)
                query = query.Where(e => e.UnitsAvailable >= request.MinUnitsAvailable.Value);

            if (request.MinPrice.HasValue)
                query = query.Where(e => e.PriceFrom.HasValue && e.PriceFrom.Value >= request.MinPrice.Value);

            if (request.MaxPrice.HasValue)
                query = query.Where(e => e.PriceFrom.HasValue && e.PriceFrom.Value <= request.MaxPrice.Value);

            var sorted = ApplySorting(query, request.SortBy, request.SortDirection);
            var paged = await Paginate<Bookazone.Domain.Entities.Equipment.Equipment>.CreateAsync(
                sorted,
                request.PageIndex,
                request.PageSize);

            var pageData = paged.Data ?? new List<Bookazone.Domain.Entities.Equipment.Equipment>();
            var equipmentIds = pageData.Select(e => e.Id).ToList();
            var priceLookup = await BuildPriceLookupAsync(equipmentIds, cancellationToken);

            var dto = pageData.Select(e =>
            {
                var priceInfo = ResolvePrice(e, priceLookup);
                var card = EquipmentConverter.ToCardDto(
                    e,
                    e.FkTenant,
                    e.FkCoverMedia,
                    priceInfo.PriceFrom,
                    priceInfo.Currency,
                    priceInfo.Unit);
                card.CoverUrl = FileUrlHelper.BuildPublicUrl(
                    httpContextAccessor.HttpContext?.Request,
                    card.CoverUrl);
                return card;
            }).ToList();

            return ApiResponse.Success(dto, pages: paged.Pages, pageIndex: paged.PageIndex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting equipment list");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var equipment = await equipmentRepository.Query()
                .AsNoTracking()
                .Include(e => e.FkTenant)
                .Include(e => e.FkCoverMedia)
                .ThenInclude(m => m.FkStoredFile)
                .FirstOrDefaultAsync(e =>
                        e.Id == equipmentId &&
                        e.Active &&
                        !e.Deleted &&
                        e.Status == EquipmentStatus.Published,
                    cancellationToken);

            if (equipment == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            var pricingRules = await equipmentPricingRuleRepository.GetByEquipmentAsync(equipmentId, cancellationToken);
            var specs = await equipmentSpecRepository.GetByEquipmentAsync(equipmentId, cancellationToken);
            var policies = await equipmentPolicyRepository.GetByEquipmentAsync(equipmentId, cancellationToken);
            var media = await equipmentMediaRepository.GetByEquipmentAsync(equipmentId, cancellationToken);

            var priceInfo = ResolvePrice(equipment, pricingRules);
            var coverMedia = ResolveCover(equipment, media);

            var mediaDto = media.Select(EquipmentConverter.ToDto).ToList();
            foreach (var item in mediaDto)
                item.Url = FileUrlHelper.BuildPublicUrl(
                               httpContextAccessor.HttpContext?.Request,
                               item.Url) ?? string.Empty;

            var detail = EquipmentConverter.ToDetailDto(
                equipment,
                equipment.FkTenant,
                coverMedia,
                pricingRules.Select(EquipmentConverter.ToDto),
                specs.Select(EquipmentConverter.ToDto),
                policies.Select(EquipmentConverter.ToDto),
                mediaDto,
                priceInfo.PriceFrom,
                priceInfo.Currency,
                priceInfo.Unit);
            detail.CoverUrl = FileUrlHelper.BuildPublicUrl(
                httpContextAccessor.HttpContext?.Request,
                detail.CoverUrl);

            return ApiResponse.Success(detail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting equipment {EquipmentId}", equipmentId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetEquipmentAvailabilityAsync(
        Guid equipmentId,
        DateTime startLocal,
        DateTime endLocal,
        EquipmentPricingUnit pricingUnit,
        int unitsRequested,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var equipment = await equipmentRepository.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(e =>
                        e.Id == equipmentId &&
                        e.Active &&
                        !e.Deleted &&
                        e.Status == EquipmentStatus.Published,
                    cancellationToken);

            if (equipment == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (unitsRequested <= 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "UnitsRequested must be greater than zero.");

            var localStart = DateTime.SpecifyKind(startLocal, DateTimeKind.Unspecified);
            var localEnd = DateTime.SpecifyKind(endLocal, DateTimeKind.Unspecified);

            if (localEnd <= localStart)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "EndLocal must be greater than StartLocal.");

            var timeZone = await ResolveTenantTimeZoneAsync(equipment.FkTenantId);
            var startUtc = ConvertLocalToUtc(localStart, timeZone);
            var endUtc = ConvertLocalToUtc(localEnd, timeZone);
            var effectiveEndUtc = ResolveEffectiveEndUtc(pricingUnit, startUtc, endUtc, timeZone);
            var effectiveEndLocal = ConvertUtcToLocal(effectiveEndUtc, timeZone);

            var availability = new VwEquipmentAvailability
            {
                EquipmentId = equipmentId,
                StartLocal = localStart,
                EndLocal = effectiveEndLocal,
                StartUtc = startUtc,
                EndUtc = effectiveEndUtc,
                PricingUnit = pricingUnit,
                UnitsAvailable = equipment.UnitsAvailable,
                UnitsRequested = unitsRequested
            };

            if (equipment.UnitsAvailable <= 0)
            {
                availability.IsAvailable = false;
                availability.StatusMessage = "No units available for this equipment.";
                return ApiResponse.Success(availability);
            }

            if (unitsRequested > equipment.UnitsAvailable)
            {
                availability.IsAvailable = false;
                availability.StatusMessage = "Requested units exceed available inventory.";
                return ApiResponse.Success(availability);
            }

            var activeOrders = await equipmentOrderRepository.Query()
                .AsNoTracking()
                .Where(o =>
                    o.FkEquipmentId == equipmentId &&
                    o.Active &&
                    !o.Deleted &&
                    o.Status != EquipmentOrderStatus.Cancelled &&
                    o.Status != EquipmentOrderStatus.Declined &&
                    o.Status != EquipmentOrderStatus.Completed &&
                    (o.StartUtc == null ||
                     o.EndUtc == null ||
                     (o.StartUtc < effectiveEndUtc && o.EndUtc > startUtc) ||
                     ((o.PricingUnit == EquipmentPricingUnit.Week ||
                       o.PricingUnit == EquipmentPricingUnit.Month ||
                       o.PricingUnit == EquipmentPricingUnit.Year) &&
                      o.StartUtc < effectiveEndUtc)))
                .ToListAsync(cancellationToken);

            var reservedUnits = activeOrders
                .Where(o => OrderOverlapsRange(o, startUtc, effectiveEndUtc, timeZone))
                .Sum(o => o.UnitsRequested <= 0 ? 1 : o.UnitsRequested);

            availability.ReservedUnits = reservedUnits;
            availability.IsAvailable = reservedUnits + unitsRequested <= equipment.UnitsAvailable;
            if (!availability.IsAvailable)
                availability.StatusMessage = "Not enough units available for the selected dates.";

            return ApiResponse.Success(availability);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting equipment availability {EquipmentId}", equipmentId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    private static IQueryable<Bookazone.Domain.Entities.Equipment.Equipment> ApplySorting(
        IQueryable<Bookazone.Domain.Entities.Equipment.Equipment> query,
        string? sortBy,
        string? sortDirection)
    {
        var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "title" => isDesc ? query.OrderByDescending(e => e.Title) : query.OrderBy(e => e.Title),
            "price" => isDesc ? query.OrderByDescending(e => e.PriceFrom) : query.OrderBy(e => e.PriceFrom),
            "city" => isDesc ? query.OrderByDescending(e => e.City) : query.OrderBy(e => e.City),
            "category" => isDesc ? query.OrderByDescending(e => e.Category) : query.OrderBy(e => e.Category),
            _ => isDesc ? query.OrderByDescending(e => e.DateCreated) : query.OrderBy(e => e.DateCreated)
        };
    }

    private async Task<Dictionary<Guid, PriceInfo>> BuildPriceLookupAsync(
        List<Guid> equipmentIds,
        CancellationToken cancellationToken)
    {
        if (equipmentIds.Count == 0)
            return new Dictionary<Guid, PriceInfo>();

        var rules = await equipmentPricingRuleRepository.Query()
            .AsNoTracking()
            .Where(r =>
                equipmentIds.Contains(r.FkEquipmentId) &&
                r.Active &&
                !r.Deleted)
            .Select(r => new { r.FkEquipmentId, r.PriceAmount, r.Currency, r.Unit, r.IsPrimary })
            .ToListAsync(cancellationToken);

        return rules
            .GroupBy(r => r.FkEquipmentId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var primary = g.FirstOrDefault(x => x.IsPrimary) ??
                                  g.OrderBy(x => x.PriceAmount).First();
                    return new PriceInfo(primary.PriceAmount, primary.Currency, primary.Unit);
                });
    }

    private static PriceInfo ResolvePrice(
        Bookazone.Domain.Entities.Equipment.Equipment equipment,
        Dictionary<Guid, PriceInfo> lookup)
    {
        if (lookup.TryGetValue(equipment.Id, out var info))
            return info;

        return new PriceInfo(equipment.PriceFrom, equipment.Currency, null);
    }

    private static PriceInfo ResolvePrice(
        Bookazone.Domain.Entities.Equipment.Equipment equipment,
        IReadOnlyCollection<EquipmentPricingRule> rules)
    {
        if (rules.Count == 0)
            return new PriceInfo(equipment.PriceFrom, equipment.Currency, null);

        var primary = rules.FirstOrDefault(r => r.IsPrimary) ??
                      rules.OrderBy(r => r.PriceAmount).First();

        return new PriceInfo(primary.PriceAmount, primary.Currency, primary.Unit);
    }

    private static EquipmentMedia? ResolveCover(Bookazone.Domain.Entities.Equipment.Equipment equipment, List<EquipmentMedia> media)
    {
        if (equipment.FkCoverMediaId.HasValue)
            return media.FirstOrDefault(m => m.Id == equipment.FkCoverMediaId.Value);

        return media.FirstOrDefault(m => m.IsCover) ?? media.FirstOrDefault();
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

    private sealed record PriceInfo(decimal? PriceFrom, string? Currency, EquipmentPricingUnit? Unit);
}
