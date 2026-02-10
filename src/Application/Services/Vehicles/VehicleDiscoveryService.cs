using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Converter;
using Bookazone.Application.DTOs.Vehicles;
using Bookazone.Application.Interfaces.Repository.Vehicles;
using Bookazone.Application.Interfaces.Services.Vehicles;
using Bookazone.Domain.Entities.Vehicles;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Services.Vehicles;

public class VehicleDiscoveryService(
    IVehicleRepository vehicleRepository,
    IVehiclePricingRuleRepository vehiclePricingRuleRepository,
    IVehicleSpecRepository vehicleSpecRepository,
    IVehiclePolicyRepository vehiclePolicyRepository,
    IVehicleMediaRepository vehicleMediaRepository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<VehicleDiscoveryService> logger)
    : IVehicleDiscoveryService
{
    public async Task<ApiResult> GetVehiclesAsync(VehicleSearchRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            request ??= new VehicleSearchRequest();

            var query = vehicleRepository.Query()
                .AsNoTracking()
                .Include(v => v.FkTenant)
                .Include(v => v.FkCoverMedia)
                .ThenInclude(m => m.FkStoredFile)
                .Where(v =>
                    v.Active &&
                    !v.Deleted &&
                    v.Status == VehicleStatus.Published);

            if (request.TenantId.HasValue)
                query = query.Where(v => v.FkTenantId == request.TenantId.Value);

            if (request.ServiceType.HasValue)
                query = query.Where(v => v.ServiceType == request.ServiceType.Value);

            if (request.VehicleType.HasValue)
                query = query.Where(v => v.Type == request.VehicleType.Value);

            if (request.MinSeats.HasValue)
                query = query.Where(v => v.Seats.HasValue && v.Seats.Value >= request.MinSeats.Value);

            if (request.Transmission.HasValue)
                query = query.Where(v => v.Transmission == request.Transmission.Value);

            if (request.FuelType.HasValue)
                query = query.Where(v => v.FuelType == request.FuelType.Value);

            if (!string.IsNullOrWhiteSpace(request.City))
            {
                var city = request.City.Trim().ToLowerInvariant();
                query = query.Where(v => v.City != null && v.City.ToLower().Contains(city));
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = request.Search.Trim().ToLowerInvariant();
                query = query.Where(v =>
                    v.Title.ToLower().Contains(term) ||
                    (v.Subtitle != null && v.Subtitle.ToLower().Contains(term)) ||
                    (v.Make != null && v.Make.ToLower().Contains(term)) ||
                    (v.Model != null && v.Model.ToLower().Contains(term)) ||
                    v.FkTenant.Name.ToLower().Contains(term));
            }

            var sorted = ApplySorting(query, request.SortBy, request.SortDirection);
            var paged = await Paginate<Vehicle>.CreateAsync(sorted, request.PageIndex, request.PageSize);

            var pageData = paged.Data ?? new List<Vehicle>();
            var vehicleIds = pageData.Select(v => v.Id).ToList();
            var priceLookup = await BuildPriceLookupAsync(vehicleIds, cancellationToken);

            var dto = pageData.Select(v =>
            {
                var priceInfo = ResolvePrice(v, priceLookup);
                var card = VehicleConverter.ToCardDto(
                    v,
                    v.FkTenant,
                    v.FkCoverMedia,
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
            logger.LogError(ex, "Error getting vehicles");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var vehicle = await vehicleRepository.Query()
                .AsNoTracking()
                .Include(v => v.FkTenant)
                .Include(v => v.FkCoverMedia)
                .ThenInclude(m => m.FkStoredFile)
                .FirstOrDefaultAsync(v =>
                    v.Id == vehicleId &&
                    v.Active &&
                    !v.Deleted &&
                    v.Status == VehicleStatus.Published,
                    cancellationToken);

            if (vehicle == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            var pricingRules = await vehiclePricingRuleRepository.GetByVehicleAsync(vehicleId, cancellationToken);
            var specs = await vehicleSpecRepository.GetByVehicleAsync(vehicleId, cancellationToken);
            var policies = await vehiclePolicyRepository.GetByVehicleAsync(vehicleId, cancellationToken);
            var media = await vehicleMediaRepository.GetByVehicleAsync(vehicleId, cancellationToken);

            var priceInfo = ResolvePrice(vehicle, pricingRules);
            var coverMedia = ResolveCover(vehicle, media);

            var mediaDto = media.Select(VehicleConverter.ToDto).ToList();
            foreach (var item in mediaDto)
                item.Url = FileUrlHelper.BuildPublicUrl(
                               httpContextAccessor.HttpContext?.Request,
                               item.Url) ?? string.Empty;

            var detail = VehicleConverter.ToDetailDto(
                vehicle,
                vehicle.FkTenant,
                coverMedia,
                pricingRules.Select(VehicleConverter.ToDto),
                specs.Select(VehicleConverter.ToDto),
                policies.Select(VehicleConverter.ToDto),
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
            logger.LogError(ex, "Error getting vehicle {VehicleId}", vehicleId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    private static IQueryable<Vehicle> ApplySorting(IQueryable<Vehicle> query, string? sortBy, string? sortDirection)
    {
        var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "title" => isDesc ? query.OrderByDescending(v => v.Title) : query.OrderBy(v => v.Title),
            "price" => isDesc ? query.OrderByDescending(v => v.PriceFrom) : query.OrderBy(v => v.PriceFrom),
            "city" => isDesc ? query.OrderByDescending(v => v.City) : query.OrderBy(v => v.City),
            _ => isDesc ? query.OrderByDescending(v => v.DateCreated) : query.OrderBy(v => v.DateCreated)
        };
    }

    private async Task<Dictionary<Guid, PriceInfo>> BuildPriceLookupAsync(
        List<Guid> vehicleIds,
        CancellationToken cancellationToken)
    {
        if (vehicleIds.Count == 0)
            return new Dictionary<Guid, PriceInfo>();

        var rules = await vehiclePricingRuleRepository.Query()
            .AsNoTracking()
            .Where(r =>
                vehicleIds.Contains(r.FkVehicleId) &&
                r.Active &&
                !r.Deleted)
            .Select(r => new { r.FkVehicleId, r.PriceAmount, r.Currency, r.Unit, r.IsPrimary })
            .ToListAsync(cancellationToken);

        return rules
            .GroupBy(r => r.FkVehicleId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var primary = g.FirstOrDefault(x => x.IsPrimary) ??
                                  g.OrderBy(x => x.PriceAmount).First();
                    return new PriceInfo(primary.PriceAmount, primary.Currency, primary.Unit);
                });
    }

    private static PriceInfo ResolvePrice(Vehicle vehicle, Dictionary<Guid, PriceInfo> lookup)
    {
        if (lookup.TryGetValue(vehicle.Id, out var info))
            return info;

        return new PriceInfo(vehicle.PriceFrom, vehicle.Currency, null);
    }

    private static PriceInfo ResolvePrice(Vehicle vehicle, IReadOnlyCollection<VehiclePricingRule> rules)
    {
        if (rules.Count == 0)
            return new PriceInfo(vehicle.PriceFrom, vehicle.Currency, null);

        var primary = rules.FirstOrDefault(r => r.IsPrimary) ??
                      rules.OrderBy(r => r.PriceAmount).First();

        return new PriceInfo(primary.PriceAmount, primary.Currency, primary.Unit);
    }

    private static VehicleMedia? ResolveCover(Vehicle vehicle, List<VehicleMedia> media)
    {
        if (vehicle.FkCoverMediaId.HasValue)
            return media.FirstOrDefault(m => m.Id == vehicle.FkCoverMediaId.Value);

        return media.FirstOrDefault(m => m.IsCover) ?? media.FirstOrDefault();
    }

    private sealed record PriceInfo(decimal? PriceFrom, string? Currency, VehiclePricingUnit? Unit);
}
