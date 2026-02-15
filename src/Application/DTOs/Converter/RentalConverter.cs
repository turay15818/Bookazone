using Bookazone.Application.DTOs.Response;
using Bookazone.Domain.Entities.Rentals;
using Bookazone.Domain.Entities.Tenant;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Converter;

public static class RentalConverter
{
    public static VwRentalPricingRule ToDto(RentalPricingRule rule)
    {
        return new VwRentalPricingRule
        {
            Id = rule.Id,
            RentalId = rule.FkRentalId,
            Unit = rule.Unit,
            PriceAmount = rule.PriceAmount,
            Currency = rule.Currency,
            MinQuantity = rule.MinQuantity,
            MaxQuantity = rule.MaxQuantity,
            IsPrimary = rule.IsPrimary
        };
    }

    public static VwRentalSpec ToDto(RentalSpec spec)
    {
        return new VwRentalSpec
        {
            Id = spec.Id,
            RentalId = spec.FkRentalId,
            Label = spec.Label,
            Value = spec.Value,
            SortOrder = spec.SortOrder
        };
    }

    public static VwRentalPolicy ToDto(RentalPolicy policy)
    {
        return new VwRentalPolicy
        {
            Id = policy.Id,
            RentalId = policy.FkRentalId,
            Title = policy.Title,
            Body = policy.Body,
            SortOrder = policy.SortOrder
        };
    }

    public static VwRentalMedia ToDto(RentalMedia media)
    {
        return new VwRentalMedia
        {
            Id = media.Id,
            RentalId = media.FkRentalId,
            StoredFileId = media.FkStoredFileId,
            Url = media.FkStoredFile?.RelativePath ?? string.Empty,
            IsCover = media.IsCover,
            SortOrder = media.SortOrder
        };
    }

    public static VwRentalCard ToCardDto(
        Rental rental,
        Tenants tenant,
        RentalMedia? coverMedia,
        decimal? priceFrom,
        string? currency,
        RentalPricingUnit? priceUnit)
    {
        return new VwRentalCard
        {
            Id = rental.Id,
            Title = rental.Title,
            Subtitle = rental.Subtitle,
            Type = rental.Type,
            City = rental.City,
            Address = rental.Address,
            Capacity = rental.Capacity,
            Bedrooms = rental.Bedrooms,
            UnitsAvailable = rental.UnitsAvailable,
            BookingMode = rental.BookingMode,
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

    public static VwRentalListItem ToListItemDto(
        Rental rental,
        Tenants tenant,
        RentalMedia? coverMedia,
        decimal? priceFrom,
        string? currency,
        RentalPricingUnit? priceUnit)
    {
        var card = ToCardDto(rental, tenant, coverMedia, priceFrom, currency, priceUnit);
        return new VwRentalListItem
        {
            Id = card.Id,
            Title = card.Title,
            Subtitle = card.Subtitle,
            Type = card.Type,
            City = card.City,
            Address = card.Address,
            Capacity = card.Capacity,
            Bedrooms = card.Bedrooms,
            UnitsAvailable = card.UnitsAvailable,
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
            Status = rental.Status
        };
    }

    public static VwRentalDetail ToDetailDto(
        Rental rental,
        Tenants tenant,
        RentalMedia? coverMedia,
        IEnumerable<VwRentalPricingRule> pricing,
        IEnumerable<VwRentalSpec> specs,
        IEnumerable<VwRentalPolicy> policies,
        IEnumerable<VwRentalMedia> media,
        decimal? priceFrom,
        string? currency,
        RentalPricingUnit? priceUnit)
    {
        return new VwRentalDetail
        {
            Id = rental.Id,
            TenantId = tenant.Id,
            TenantName = tenant.Name,
            TenantAddress = tenant.Address,
            TenantCity = tenant.City,
            TenantState = tenant.State,
            TenantCountry = tenant.Country,
            TenantLogo = tenant.Logo,
            TenantVerified = tenant.IsVerified,
            Type = rental.Type,
            Title = rental.Title,
            Subtitle = rental.Subtitle,
            Description = rental.Description,
            City = rental.City,
            Address = rental.Address,
            Latitude = rental.Latitude,
            Longitude = rental.Longitude,
            Capacity = rental.Capacity,
            Bedrooms = rental.Bedrooms,
            Bathrooms = rental.Bathrooms,
            FloorAreaSquareMeters = rental.FloorAreaSquareMeters,
            UnitsAvailable = rental.UnitsAvailable,
            HasToilet = rental.HasToilet,
            HasAirConditioning = rental.HasAirConditioning,
            HasFan = rental.HasFan,
            HasSoundSystem = rental.HasSoundSystem,
            HasWifi = rental.HasWifi,
            HasParking = rental.HasParking,
            MinBookingDuration = rental.MinBookingDuration,
            MaxBookingDuration = rental.MaxBookingDuration,
            BookingDurationUnit = rental.BookingDurationUnit,
            CheckInTime = rental.CheckInTime,
            CheckOutTime = rental.CheckOutTime,
            BookingMode = rental.BookingMode,
            Status = rental.Status,
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

    private static string? FormatPriceLabel(decimal? priceFrom, string? currency, RentalPricingUnit? unit)
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

    private static string FormatUnit(RentalPricingUnit unit)
    {
        return unit switch
        {
            RentalPricingUnit.Hour => "hour",
            RentalPricingUnit.Day => "day",
            RentalPricingUnit.Week => "week",
            RentalPricingUnit.Month => "month",
            RentalPricingUnit.Year => "year",
            _ => "unit"
        };
    }
}
