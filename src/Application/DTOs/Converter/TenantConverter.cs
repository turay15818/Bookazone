using Bookazone.Application.DTOs.Response;
using Bookazone.Domain.Entities.Tenant;

namespace Bookazone.Application.DTOs.Converter;

public static class TenantConverter
{
    public static VwTenant ToDto(Tenants tenant)
    {
        return new VwTenant
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Email = tenant.Email,
            Phone = tenant.Phone,
            Address = tenant.Address,
            Active = tenant.Active,
            IsVerified = tenant.IsVerified,
            DateCreated = tenant.DateCreated
        };
    }
}
