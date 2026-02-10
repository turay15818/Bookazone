using Bookazone.Application.DTOs.Response;
using Bookazone.Domain.Entities.Booking;
using Bookazone.Domain.Entities.Sports;
using Bookazone.Domain.Entities.Tenant;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Converter;

public static class BookingConfigConverter
{
    public static VwBookingCategory ToDto(BookingCategory category)
    {
        return new VwBookingCategory
        {
            Id = category.Id,
            Name = category.Name,
            Code = category.Code,
            Description = category.Description,
            DefaultBookingMode = category.DefaultBookingMode,
            Active = category.Active
        };
    }

    public static VwTenantBookingCategory ToDto(
        TenantBookingCategory tenantCategory,
        BookingCategory? categoryOverride = null)
    {
        var category = categoryOverride ?? tenantCategory.FkBookingCategory;
        var defaultMode = category?.DefaultBookingMode ?? BookingMode.Instant;

        return new VwTenantBookingCategory
        {
            Id = tenantCategory.Id,
            TenantId = tenantCategory.FkTenantId,
            BookingCategoryId = tenantCategory.FkBookingCategoryId,
            BookingCategoryName = category?.Name ?? string.Empty,
            BookingCategoryCode = category?.Code ?? string.Empty,
            DefaultBookingMode = defaultMode,
            IsEnabled = tenantCategory.IsEnabled,
            BookingModeOverride = tenantCategory.BookingModeOverride,
            EffectiveBookingMode = tenantCategory.BookingModeOverride ?? defaultMode
        };
    }

    public static VwTenantBookingCategory ToDto(TenantBookingCategory tenantCategory)
    {
        return ToDto(tenantCategory, tenantCategory.FkBookingCategory);
    }

    public static VwTenantWorkingHour ToDto(TenantWorkingHour workingHour)
    {
        return new VwTenantWorkingHour
        {
            Id = workingHour.Id,
            TenantId = workingHour.FkTenantId,
            DayOfWeek = workingHour.DayOfWeek,
            OpenTime = workingHour.OpenTime,
            CloseTime = workingHour.CloseTime,
            IsClosed = workingHour.IsClosed,
            SlotDurationMinutes = workingHour.SlotDurationMinutes
        };
    }

    public static VwTenantSportType ToDto(TenantSportType type)
    {
        return new VwTenantSportType
        {
            Id = type.Id,
            TenantId = type.FkTenantId,
            TenantBookingCategoryId = type.FkTenantBookingCategoryId,
            Name = type.Name,
            Description = type.Description,
            BookingMode = type.BookingMode,
            MinDurationMinutes = type.MinDurationMinutes,
            MaxDurationMinutes = type.MaxDurationMinutes,
            BufferMinutes = type.BufferMinutes
        };
    }

    public static VwSportResource ToDto(SportResource resource)
    {
        return new VwSportResource
        {
            Id = resource.Id,
            TenantId = resource.FkTenantId,
            TenantSportTypeId = resource.FkTenantSportTypeId,
            Name = resource.Name,
            Description = resource.Description,
            Capacity = resource.Capacity,
            Address = resource.Address,
            Latitude = resource.Latitude,
            Longitude = resource.Longitude
        };
    }

    public static VwSportResourceAvailability ToDto(SportResourceAvailability availability)
    {
        return new VwSportResourceAvailability
        {
            Id = availability.Id,
            ResourceId = availability.FkSportResourceId,
            DayOfWeek = availability.DayOfWeek,
            OpenTime = availability.OpenTime,
            CloseTime = availability.CloseTime,
            IsClosed = availability.IsClosed
        };
    }

    public static VwSportResourceBlackout ToDto(SportResourceBlackout blackout)
    {
        return new VwSportResourceBlackout
        {
            Id = blackout.Id,
            ResourceId = blackout.FkSportResourceId,
            StartUtc = blackout.StartUtc,
            EndUtc = blackout.EndUtc,
            Reason = blackout.Reason
        };
    }

    public static VwSportResourcePricingRule ToDto(SportResourcePricingRule rule)
    {
        return new VwSportResourcePricingRule
        {
            Id = rule.Id,
            ResourceId = rule.FkSportResourceId,
            RuleType = rule.RuleType,
            PriceAmount = rule.PriceAmount,
            Currency = rule.Currency,
            DayOfWeek = rule.DayOfWeek,
            StartTime = rule.StartTime,
            EndTime = rule.EndTime,
            MinDurationMinutes = rule.MinDurationMinutes
        };
    }
}
