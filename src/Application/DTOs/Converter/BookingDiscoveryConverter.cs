using Bookazone.Application.DTOs.Response;
using Bookazone.Domain.Entities.Sports;
using Bookazone.Domain.Entities.Tenant;

namespace Bookazone.Application.DTOs.Converter;

public static class BookingDiscoveryConverter
{
    public static VwSportTypePublic ToPublicDto(TenantSportType type)
    {
        return new VwSportTypePublic
        {
            Id = type.Id,
            TenantId = type.FkTenantId,
            TenantName = type.FkTenant.Name,
            Name = type.Name,
            Description = type.Description,
            BookingMode = type.BookingMode,
            MinDurationMinutes = type.MinDurationMinutes,
            MaxDurationMinutes = type.MaxDurationMinutes,
            BufferMinutes = type.BufferMinutes
        };
    }

    public static VwTenantPublic ToPublicDto(Tenants tenant)
    {
        return new VwTenantPublic
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Description = tenant.Description,
            Address = tenant.Address,
            City = tenant.City,
            State = tenant.State,
            Country = tenant.Country,
            Logo = tenant.Logo,
            IsVerified = tenant.IsVerified
        };
    }

    public static VwSportResourcePublic ToPublicDto(
        SportResource resource,
        IEnumerable<VwSportResourceAvailability>? availability = null)
    {
        var tenant = resource.FkTenant;
        var sportType = resource.FkTenantSportType;

        return new VwSportResourcePublic
        {
            Id = resource.Id,
            Name = resource.Name,
            Description = resource.Description,
            Capacity = resource.Capacity,
            Address = resource.Address,
            Latitude = resource.Latitude,
            Longitude = resource.Longitude,
            TenantId = resource.FkTenantId,
            TenantName = tenant.Name,
            TenantAddress = tenant.Address,
            TenantCity = tenant.City,
            TenantState = tenant.State,
            TenantCountry = tenant.Country,
            TenantLogo = tenant.Logo,
            TenantVerified = tenant.IsVerified,
            SportTypeId = resource.FkTenantSportTypeId,
            SportTypeName = sportType.Name,
            AvailabilityHours = availability?.ToList() ?? new List<VwSportResourceAvailability>()
        };
    }
}
