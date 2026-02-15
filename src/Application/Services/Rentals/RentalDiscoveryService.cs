using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Converter;
using Bookazone.Application.DTOs.Rentals;
using Bookazone.Application.DTOs.Response;
using Bookazone.Application.Interfaces.Repository.Rentals;
using Bookazone.Application.Interfaces.Repository.Subscription;
using Bookazone.Application.Interfaces.Repository.Tenant;
using Bookazone.Application.Interfaces.Services.Rentals;
using Bookazone.Domain.Entities.Rentals;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Services.Rentals;

public class RentalDiscoveryService(
    IRentalRepository rentalRepository,
    IRentalPricingRuleRepository rentalPricingRuleRepository,
    IRentalSpecRepository rentalSpecRepository,
    IRentalPolicyRepository rentalPolicyRepository,
    IRentalMediaRepository rentalMediaRepository,
    IRentalOrderRepository rentalOrderRepository,
    ITenantWorkingHourRepository tenantWorkingHourRepository,
    ITenantSettingsRepository tenantSettingsRepository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<RentalDiscoveryService> logger)
    : IRentalDiscoveryService
{
    public async Task<ApiResult> GetRentalsAsync(RentalSearchRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            request ??= new RentalSearchRequest();

            var query = rentalRepository.Query()
                .AsNoTracking()
                .Include(r => r.FkTenant)
                .Include(r => r.FkCoverMedia)
                .ThenInclude(m => m.FkStoredFile)
                .Where(r =>
                    r.Active &&
                    !r.Deleted &&
                    r.Status == RentalStatus.Published);

            if (request.TenantId.HasValue)
                query = query.Where(r => r.FkTenantId == request.TenantId.Value);

            if (request.Type.HasValue)
                query = query.Where(r => r.Type == request.Type.Value);

            if (request.MinCapacity.HasValue)
                query = query.Where(r => r.Capacity.HasValue && r.Capacity.Value >= request.MinCapacity.Value);

            if (request.MinBedrooms.HasValue)
                query = query.Where(r => r.Bedrooms.HasValue && r.Bedrooms.Value >= request.MinBedrooms.Value);

            if (request.MinBathrooms.HasValue)
                query = query.Where(r => r.Bathrooms.HasValue && r.Bathrooms.Value >= request.MinBathrooms.Value);

            if (request.MinPrice.HasValue)
                query = query.Where(r => r.PriceFrom.HasValue && r.PriceFrom.Value >= request.MinPrice.Value);

            if (request.MaxPrice.HasValue)
                query = query.Where(r => r.PriceFrom.HasValue && r.PriceFrom.Value <= request.MaxPrice.Value);

            if (!string.IsNullOrWhiteSpace(request.City))
            {
                var city = request.City.Trim().ToLowerInvariant();
                query = query.Where(r => r.City != null && r.City.ToLower().Contains(city));
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = request.Search.Trim().ToLowerInvariant();
                query = query.Where(r =>
                    r.Title.ToLower().Contains(term) ||
                    (r.Subtitle != null && r.Subtitle.ToLower().Contains(term)) ||
                    (r.Address != null && r.Address.ToLower().Contains(term)) ||
                    (r.City != null && r.City.ToLower().Contains(term)) ||
                    r.FkTenant.Name.ToLower().Contains(term));
            }

            var sorted = ApplySorting(query, request.SortBy, request.SortDirection);
            var paged = await Paginate<Rental>.CreateAsync(sorted, request.PageIndex, request.PageSize);

            var pageData = paged.Data ?? new List<Rental>();
            var rentalIds = pageData.Select(r => r.Id).ToList();
            var priceLookup = await BuildPriceLookupAsync(rentalIds, cancellationToken);

            var dto = pageData.Select(r =>
            {
                var priceInfo = ResolvePrice(r, priceLookup);
                var card = RentalConverter.ToCardDto(
                    r,
                    r.FkTenant,
                    r.FkCoverMedia,
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
            logger.LogError(ex, "Error getting rentals");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetRentalAsync(Guid rentalId, CancellationToken cancellationToken = default)
    {
        try
        {
            var rental = await rentalRepository.Query()
                .AsNoTracking()
                .Include(r => r.FkTenant)
                .Include(r => r.FkCoverMedia)
                .ThenInclude(m => m.FkStoredFile)
                .FirstOrDefaultAsync(r =>
                        r.Id == rentalId &&
                        r.Active &&
                        !r.Deleted &&
                        r.Status == RentalStatus.Published,
                    cancellationToken);

            if (rental == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            var pricingRules = await rentalPricingRuleRepository.GetByRentalAsync(rentalId, cancellationToken);
            var specs = await rentalSpecRepository.GetByRentalAsync(rentalId, cancellationToken);
            var policies = await rentalPolicyRepository.GetByRentalAsync(rentalId, cancellationToken);
            var media = await rentalMediaRepository.GetByRentalAsync(rentalId, cancellationToken);

            var priceInfo = ResolvePrice(rental, pricingRules);
            var coverMedia = ResolveCover(rental, media);

            var mediaDto = media.Select(RentalConverter.ToDto).ToList();
            foreach (var item in mediaDto)
                item.Url = FileUrlHelper.BuildPublicUrl(
                               httpContextAccessor.HttpContext?.Request,
                               item.Url) ?? string.Empty;

            var detail = RentalConverter.ToDetailDto(
                rental,
                rental.FkTenant,
                coverMedia,
                pricingRules.Select(RentalConverter.ToDto),
                specs.Select(RentalConverter.ToDto),
                policies.Select(RentalConverter.ToDto),
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
            logger.LogError(ex, "Error getting rental {RentalId}", rentalId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetRentalSlotsAsync(
        Guid rentalId,
        DateTime dateLocal,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var rental = await rentalRepository.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(r =>
                        r.Id == rentalId &&
                        r.Active &&
                        !r.Deleted &&
                        r.Status == RentalStatus.Published,
                    cancellationToken);

            if (rental == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            var localDate = dateLocal.Date;
            var dayOfWeek = localDate.DayOfWeek;
            var tenantId = rental.FkTenantId;

            var tenantHours = await tenantWorkingHourRepository.GetByTenantAndDayAsync(
                tenantId,
                dayOfWeek,
                cancellationToken);

            var summary = new VwRentalSlotSummary
            {
                RentalId = rentalId,
                DateLocal = localDate,
                DayOfWeek = dayOfWeek,
                UnitsAvailable = rental.UnitsAvailable
            };

            if (tenantHours == null || tenantHours.IsClosed || tenantHours.OpenTime == null || tenantHours.CloseTime == null)
            {
                summary.IsClosed = true;
                summary.StatusMessage = "Tenant working hours are closed or not configured for this day.";
                return ApiResponse.Success(summary);
            }

            summary.OpenTime = tenantHours.OpenTime.Value;
            summary.CloseTime = tenantHours.CloseTime.Value;
            summary.SlotDurationMinutes = tenantHours.SlotDurationMinutes;

            if (tenantHours.SlotDurationMinutes <= 0 || summary.OpenTime >= summary.CloseTime)
            {
                summary.IsClosed = true;
                summary.StatusMessage = "No available slots for this day.";
                return ApiResponse.Success(summary);
            }

            if (rental.UnitsAvailable <= 0)
            {
                summary.IsClosed = true;
                summary.StatusMessage = "No units available for this rental.";
                return ApiResponse.Success(summary);
            }

            summary.IsClosed = false;

            var timeZone = await ResolveTenantTimeZoneAsync(tenantId);
            var dayStartLocal = DateTime.SpecifyKind(localDate, DateTimeKind.Unspecified);
            var dayEndLocal = DateTime.SpecifyKind(localDate.AddDays(1), DateTimeKind.Unspecified);
            var dayStartUtc = ConvertLocalToUtc(dayStartLocal, timeZone);
            var dayEndUtc = ConvertLocalToUtc(dayEndLocal, timeZone);

            var activeOrders = await rentalOrderRepository.Query()
                .AsNoTracking()
                .Where(o =>
                    o.FkRentalId == rentalId &&
                    o.Active &&
                    !o.Deleted &&
                    o.Status != RentalOrderStatus.Cancelled &&
                    o.Status != RentalOrderStatus.Declined &&
                    o.Status != RentalOrderStatus.Completed &&
                    (o.StartUtc == null ||
                     o.EndUtc == null ||
                     (o.StartUtc < dayEndUtc && o.EndUtc > dayStartUtc) ||
                     ((o.PricingUnit == RentalPricingUnit.Week ||
                       o.PricingUnit == RentalPricingUnit.Month ||
                       o.PricingUnit == RentalPricingUnit.Year) &&
                      o.StartUtc < dayEndUtc)))
                .ToListAsync(cancellationToken);

            var slotDuration = TimeSpan.FromMinutes(tenantHours.SlotDurationMinutes);
            var slotStartLocal = dayStartLocal.Add(summary.OpenTime.Value);
            var lastSlotEndLocal = dayStartLocal.Add(summary.CloseTime.Value);

            while (slotStartLocal + slotDuration <= lastSlotEndLocal)
            {
                var slotEndLocal = slotStartLocal + slotDuration;
                var slotStartUtc = ConvertLocalToUtc(slotStartLocal, timeZone);
                var slotEndUtc = ConvertLocalToUtc(slotEndLocal, timeZone);

                var reservedUnits = activeOrders
                    .Where(o => OrderOverlapsSlot(o, slotStartUtc, slotEndUtc, timeZone))
                    .Sum(o => o.UnitsRequested <= 0 ? 1 : o.UnitsRequested);

                var isAvailable = reservedUnits < rental.UnitsAvailable;

                summary.Slots.Add(new VwRentalSlot
                {
                    StartLocal = slotStartLocal,
                    EndLocal = slotEndLocal,
                    StartUtc = slotStartUtc,
                    EndUtc = slotEndUtc,
                    IsAvailable = isAvailable,
                    UnavailableReason = isAvailable ? null : "Booked",
                    ReservedUnits = reservedUnits
                });

                slotStartLocal = slotEndLocal;
            }

            return ApiResponse.Success(summary);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting rental slots {RentalId}", rentalId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    private static IQueryable<Rental> ApplySorting(IQueryable<Rental> query, string? sortBy, string? sortDirection)
    {
        var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "title" => isDesc ? query.OrderByDescending(r => r.Title) : query.OrderBy(r => r.Title),
            "price" => isDesc ? query.OrderByDescending(r => r.PriceFrom) : query.OrderBy(r => r.PriceFrom),
            "city" => isDesc ? query.OrderByDescending(r => r.City) : query.OrderBy(r => r.City),
            _ => isDesc ? query.OrderByDescending(r => r.DateCreated) : query.OrderBy(r => r.DateCreated)
        };
    }

    private async Task<Dictionary<Guid, PriceInfo>> BuildPriceLookupAsync(
        List<Guid> rentalIds,
        CancellationToken cancellationToken)
    {
        if (rentalIds.Count == 0)
            return new Dictionary<Guid, PriceInfo>();

        var rules = await rentalPricingRuleRepository.Query()
            .AsNoTracking()
            .Where(r =>
                rentalIds.Contains(r.FkRentalId) &&
                r.Active &&
                !r.Deleted)
            .Select(r => new { r.FkRentalId, r.PriceAmount, r.Currency, r.Unit, r.IsPrimary })
            .ToListAsync(cancellationToken);

        return rules
            .GroupBy(r => r.FkRentalId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var primary = g.FirstOrDefault(x => x.IsPrimary) ??
                                  g.OrderBy(x => x.PriceAmount).First();
                    return new PriceInfo(primary.PriceAmount, primary.Currency, primary.Unit);
                });
    }

    private static PriceInfo ResolvePrice(Rental rental, Dictionary<Guid, PriceInfo> lookup)
    {
        if (lookup.TryGetValue(rental.Id, out var info))
            return info;

        return new PriceInfo(rental.PriceFrom, rental.Currency, null);
    }

    private static PriceInfo ResolvePrice(Rental rental, IReadOnlyCollection<RentalPricingRule> rules)
    {
        if (rules.Count == 0)
            return new PriceInfo(rental.PriceFrom, rental.Currency, null);

        var primary = rules.FirstOrDefault(r => r.IsPrimary) ??
                      rules.OrderBy(r => r.PriceAmount).First();

        return new PriceInfo(primary.PriceAmount, primary.Currency, primary.Unit);
    }

    private static RentalMedia? ResolveCover(Rental rental, List<RentalMedia> media)
    {
        if (rental.FkCoverMediaId.HasValue)
            return media.FirstOrDefault(m => m.Id == rental.FkCoverMediaId.Value);

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

    private static bool OrderOverlapsSlot(
        RentalOrder order,
        DateTime slotStartUtc,
        DateTime slotEndUtc,
        TimeZoneInfo timeZone)
    {
        if (!order.StartUtc.HasValue || !order.EndUtc.HasValue)
            return true;

        var effectiveEndUtc = ResolveEffectiveEndUtc(order, timeZone);
        return order.StartUtc.Value < slotEndUtc && effectiveEndUtc > slotStartUtc;
    }

    private static DateTime ResolveEffectiveEndUtc(RentalOrder order, TimeZoneInfo timeZone)
    {
        if (!order.StartUtc.HasValue || !order.EndUtc.HasValue)
            return order.EndUtc ?? DateTime.MaxValue;

        var localStart = ConvertUtcToLocal(order.StartUtc.Value, timeZone);

        return order.PricingUnit switch
        {
            RentalPricingUnit.Week => ConvertLocalToUtc(GetStartOfNextWeekLocal(localStart), timeZone),
            RentalPricingUnit.Month => ConvertLocalToUtc(GetStartOfNextMonthLocal(localStart), timeZone),
            RentalPricingUnit.Year => ConvertLocalToUtc(GetStartOfNextYearLocal(localStart), timeZone),
            _ => order.EndUtc.Value
        };
    }

    private static DateTime GetStartOfNextWeekLocal(DateTime localStart)
    {
        var daysUntilNextSunday = localStart.DayOfWeek == DayOfWeek.Sunday
            ? 7
            : 7 - (int)localStart.DayOfWeek;
        return localStart.Date.AddDays(daysUntilNextSunday);
    }

    private static DateTime GetStartOfNextMonthLocal(DateTime localStart)
        => new DateTime(localStart.Year, localStart.Month, 1, 0, 0, 0).AddMonths(1);

    private static DateTime GetStartOfNextYearLocal(DateTime localStart)
        => new DateTime(localStart.Year, 1, 1, 0, 0, 0).AddYears(1);

    private sealed record PriceInfo(decimal? PriceFrom, string? Currency, RentalPricingUnit? Unit);
}
