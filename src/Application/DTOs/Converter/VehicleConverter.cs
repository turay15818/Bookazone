using Bookazone.Application.DTOs.Response;
using Bookazone.Domain.Entities.Tenant;
using Bookazone.Domain.Entities.Vehicles;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Converter;

public static class VehicleConverter
{
    public static VwVehiclePricingRule ToDto(VehiclePricingRule rule)
    {
        return new VwVehiclePricingRule
        {
            Id = rule.Id,
            VehicleId = rule.FkVehicleId,
            Unit = rule.Unit,
            PriceAmount = rule.PriceAmount,
            Currency = rule.Currency,
            MinQuantity = rule.MinQuantity,
            MaxQuantity = rule.MaxQuantity,
            IsPrimary = rule.IsPrimary
        };
    }

    public static VwVehicleSpec ToDto(VehicleSpec spec)
    {
        return new VwVehicleSpec
        {
            Id = spec.Id,
            VehicleId = spec.FkVehicleId,
            Label = spec.Label,
            Value = spec.Value,
            SortOrder = spec.SortOrder
        };
    }

    public static VwVehiclePolicy ToDto(VehiclePolicy policy)
    {
        return new VwVehiclePolicy
        {
            Id = policy.Id,
            VehicleId = policy.FkVehicleId,
            Title = policy.Title,
            Body = policy.Body,
            SortOrder = policy.SortOrder
        };
    }

    public static VwVehicleMedia ToDto(VehicleMedia media)
    {
        return new VwVehicleMedia
        {
            Id = media.Id,
            VehicleId = media.FkVehicleId,
            StoredFileId = media.FkStoredFileId,
            Url = media.FkStoredFile?.RelativePath ?? string.Empty,
            IsCover = media.IsCover,
            SortOrder = media.SortOrder
        };
    }

    public static VwVehicleCard ToCardDto(
        Vehicle vehicle,
        Tenants tenant,
        VehicleMedia? coverMedia,
        decimal? priceFrom,
        string? currency,
        VehiclePricingUnit? priceUnit)
    {
        return new VwVehicleCard
        {
            Id = vehicle.Id,
            Title = vehicle.Title,
            Subtitle = vehicle.Subtitle,
            ServiceType = vehicle.ServiceType,
            Type = vehicle.Type,
            City = vehicle.City,
            Address = vehicle.Address,
            Seats = vehicle.Seats,
            Transmission = vehicle.Transmission,
            FuelType = vehicle.FuelType,
            BookingMode = vehicle.BookingMode,
            PriceFrom = priceFrom,
            Currency = currency,
            PriceUnit = priceUnit,
            PriceLabel = FormatPriceLabel(priceFrom, currency, priceUnit),
            CoverUrl = coverMedia?.FkStoredFile?.RelativePath,
            TenantId = tenant.Id,
            TenantName = tenant.Name,
            TenantLogo = tenant.Logo,
            TenantVerified = tenant.IsVerified
        };
    }

    public static VwVehicleListItem ToListItemDto(
        Vehicle vehicle,
        Tenants tenant,
        VehicleMedia? coverMedia,
        decimal? priceFrom,
        string? currency,
        VehiclePricingUnit? priceUnit)
    {
        var card = ToCardDto(vehicle, tenant, coverMedia, priceFrom, currency, priceUnit);
        return new VwVehicleListItem
        {
            Id = card.Id,
            Title = card.Title,
            Subtitle = card.Subtitle,
            ServiceType = card.ServiceType,
            Type = card.Type,
            City = card.City,
            Address = card.Address,
            Seats = card.Seats,
            Transmission = card.Transmission,
            FuelType = card.FuelType,
            BookingMode = card.BookingMode,
            PriceFrom = card.PriceFrom,
            Currency = card.Currency,
            PriceUnit = card.PriceUnit,
            PriceLabel = card.PriceLabel,
            CoverUrl = card.CoverUrl,
            TenantId = card.TenantId,
            TenantName = card.TenantName,
            TenantLogo = card.TenantLogo,
            TenantVerified = card.TenantVerified,
            Status = vehicle.Status
        };
    }

    public static VwVehicleDetail ToDetailDto(
        Vehicle vehicle,
        Tenants tenant,
        VehicleMedia? coverMedia,
        IEnumerable<VwVehiclePricingRule> pricing,
        IEnumerable<VwVehicleSpec> specs,
        IEnumerable<VwVehiclePolicy> policies,
        IEnumerable<VwVehicleMedia> media,
        decimal? priceFrom,
        string? currency,
        VehiclePricingUnit? priceUnit)
    {
        return new VwVehicleDetail
        {
            Id = vehicle.Id,
            TenantId = tenant.Id,
            TenantName = tenant.Name,
            TenantAddress = tenant.Address,
            TenantCity = tenant.City,
            TenantState = tenant.State,
            TenantCountry = tenant.Country,
            TenantLogo = tenant.Logo,
            TenantVerified = tenant.IsVerified,
            ServiceType = vehicle.ServiceType,
            Type = vehicle.Type,
            Title = vehicle.Title,
            Subtitle = vehicle.Subtitle,
            Description = vehicle.Description,
            City = vehicle.City,
            Address = vehicle.Address,
            Latitude = vehicle.Latitude,
            Longitude = vehicle.Longitude,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Seats = vehicle.Seats,
            Transmission = vehicle.Transmission,
            FuelType = vehicle.FuelType,
            HasAirConditioning = vehicle.HasAirConditioning,
            HasInsurance = vehicle.HasInsurance,
            DeliveryAvailable = vehicle.DeliveryAvailable,
            DriverOption = vehicle.DriverOption,
            FuelPolicy = vehicle.FuelPolicy,
            MinBookingDuration = vehicle.MinBookingDuration,
            MaxBookingDuration = vehicle.MaxBookingDuration,
            BookingDurationUnit = vehicle.BookingDurationUnit,
            MaxLoadTons = vehicle.MaxLoadTons,
            CargoVolumeCubicMeters = vehicle.CargoVolumeCubicMeters,
            BookingMode = vehicle.BookingMode,
            Status = vehicle.Status,
            PriceFrom = priceFrom,
            Currency = currency,
            PriceUnit = priceUnit,
            PriceLabel = FormatPriceLabel(priceFrom, currency, priceUnit),
            CoverUrl = coverMedia?.FkStoredFile?.RelativePath,
            Pricing = pricing.ToList(),
            Specs = specs.ToList(),
            Policies = policies.ToList(),
            Media = media.ToList()
        };
    }

    private static string? FormatPriceLabel(decimal? priceFrom, string? currency, VehiclePricingUnit? unit)
    {
        if (!priceFrom.HasValue)
            return null;

        var formattedCurrency = string.IsNullOrWhiteSpace(currency)
            ? string.Empty
            : currency.Trim().ToUpperInvariant();
        var unitLabel = unit.HasValue ? FormatUnit(unit.Value) : null;
        var price = priceFrom.Value.ToString("0.##");

        if (string.IsNullOrWhiteSpace(unitLabel))
            return $"{formattedCurrency} {price}".Trim();

        return $"{formattedCurrency} {price}/{unitLabel}".Trim();
    }

    private static string FormatUnit(VehiclePricingUnit unit)
    {
        return unit switch
        {
            VehiclePricingUnit.Hour => "hour",
            VehiclePricingUnit.Day => "day",
            VehiclePricingUnit.Trip => "trip",
            VehiclePricingUnit.Kilometer => "km",
            VehiclePricingUnit.Ton => "ton",
            _ => "unit"
        };
    }
}
