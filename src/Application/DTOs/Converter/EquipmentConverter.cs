using Bookazone.Application.DTOs.Response;
using Bookazone.Domain.Entities.Equipment;
using EquipmentEntity = Bookazone.Domain.Entities.Equipment.Equipment;
using Bookazone.Domain.Entities.Tenant;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Converter;

public static class EquipmentConverter
{
    public static VwEquipmentPricingRule ToDto(EquipmentPricingRule rule)
    {
        return new VwEquipmentPricingRule
        {
            Id = rule.Id,
            EquipmentId = rule.FkEquipmentId,
            Unit = rule.Unit,
            PriceAmount = rule.PriceAmount,
            Currency = rule.Currency,
            MinQuantity = rule.MinQuantity,
            MaxQuantity = rule.MaxQuantity,
            IsPrimary = rule.IsPrimary
        };
    }

    public static VwEquipmentSpec ToDto(EquipmentSpec spec)
    {
        return new VwEquipmentSpec
        {
            Id = spec.Id,
            EquipmentId = spec.FkEquipmentId,
            Label = spec.Label,
            Value = spec.Value,
            SortOrder = spec.SortOrder
        };
    }

    public static VwEquipmentPolicy ToDto(EquipmentPolicy policy)
    {
        return new VwEquipmentPolicy
        {
            Id = policy.Id,
            EquipmentId = policy.FkEquipmentId,
            Title = policy.Title,
            Body = policy.Body,
            SortOrder = policy.SortOrder
        };
    }

    public static VwEquipmentMedia ToDto(EquipmentMedia media)
    {
        return new VwEquipmentMedia
        {
            Id = media.Id,
            EquipmentId = media.FkEquipmentId,
            StoredFileId = media.FkStoredFileId,
            Url = media.FkStoredFile?.RelativePath ?? string.Empty,
            IsCover = media.IsCover,
            SortOrder = media.SortOrder
        };
    }

    public static VwEquipmentCard ToCardDto(
        EquipmentEntity equipment,
        Tenants tenant,
        EquipmentMedia? coverMedia,
        decimal? priceFrom,
        string? currency,
        EquipmentPricingUnit? priceUnit)
    {
        return new VwEquipmentCard
        {
            Id = equipment.Id,
            Title = equipment.Title,
            Subtitle = equipment.Subtitle,
            Category = equipment.Category,
            City = equipment.City,
            Address = equipment.Address,
            UnitsAvailable = equipment.UnitsAvailable,
            BookingMode = equipment.BookingMode,
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

    public static VwEquipmentListItem ToListItemDto(
        EquipmentEntity equipment,
        Tenants tenant,
        EquipmentMedia? coverMedia,
        decimal? priceFrom,
        string? currency,
        EquipmentPricingUnit? priceUnit)
    {
        var card = ToCardDto(equipment, tenant, coverMedia, priceFrom, currency, priceUnit);
        return new VwEquipmentListItem
        {
            Id = card.Id,
            Title = card.Title,
            Subtitle = card.Subtitle,
            Category = card.Category,
            City = card.City,
            Address = card.Address,
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
            Status = equipment.Status
        };
    }

    public static VwEquipmentDetail ToDetailDto(
        EquipmentEntity equipment,
        Tenants tenant,
        EquipmentMedia? coverMedia,
        IEnumerable<VwEquipmentPricingRule> pricing,
        IEnumerable<VwEquipmentSpec> specs,
        IEnumerable<VwEquipmentPolicy> policies,
        IEnumerable<VwEquipmentMedia> media,
        decimal? priceFrom,
        string? currency,
        EquipmentPricingUnit? priceUnit)
    {
        return new VwEquipmentDetail
        {
            Id = equipment.Id,
            TenantId = tenant.Id,
            TenantName = tenant.Name,
            TenantAddress = tenant.Address,
            TenantCity = tenant.City,
            TenantState = tenant.State,
            TenantCountry = tenant.Country,
            TenantLogo = tenant.Logo,
            TenantVerified = tenant.IsVerified,
            Title = equipment.Title,
            Subtitle = equipment.Subtitle,
            Description = equipment.Description,
            Category = equipment.Category,
            City = equipment.City,
            Address = equipment.Address,
            Latitude = equipment.Latitude,
            Longitude = equipment.Longitude,
            UnitsAvailable = equipment.UnitsAvailable,
            DeliveryAvailable = equipment.DeliveryAvailable,
            PickupAvailable = equipment.PickupAvailable,
            MinBookingDuration = equipment.MinBookingDuration,
            MaxBookingDuration = equipment.MaxBookingDuration,
            BookingDurationUnit = equipment.BookingDurationUnit,
            BookingMode = equipment.BookingMode,
            Status = equipment.Status,
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

    private static string? FormatPriceLabel(decimal? priceFrom, string? currency, EquipmentPricingUnit? unit)
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

    private static string FormatUnit(EquipmentPricingUnit unit)
    {
        return unit switch
        {
            EquipmentPricingUnit.Hour => "hour",
            EquipmentPricingUnit.Day => "day",
            EquipmentPricingUnit.Week => "week",
            EquipmentPricingUnit.Month => "month",
            EquipmentPricingUnit.Year => "year",
            _ => "unit"
        };
    }
}
