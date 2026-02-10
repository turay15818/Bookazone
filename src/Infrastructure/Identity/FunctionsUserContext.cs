using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Bookazone.Application.Interfaces.Context;


namespace Bookazone.Infrastructure.Identity;

public class FunctionsUserContext(IHttpContextAccessor http) : IUserContext
{
    public string? Username =>
        http.HttpContext?.User?.Identity?.Name ??
        http.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
    public Guid? UserId
    {
        get
        {
            var user = http.HttpContext?.User;
            if (user == null) return null;
            var claim =
                user.FindFirst("sub") ??
                user.FindFirst("userId") ??
                user.FindFirst(ClaimTypes.NameIdentifier) ??
                user.FindFirst("uid");
            return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
        }
    }

    public Guid? ShopId
    {
        get
        {
            var user = http.HttpContext?.User; if (user == null) return null;
            var claim = user.FindFirst("shop_id");
            return claim != null && Guid.TryParse(claim.Value, out var id)
                ? id : null;
        }
    }

    public Guid? TenantId
    {
        get
        {
            var user = http.HttpContext?.User;
            if (user == null) return null;
            var claim = user.FindFirst("tenant_id");
            return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
        }
    }
    public string? TenantCode
    {
        get
        {
            var user = http.HttpContext?.User;
            if (user == null) return null;
            var claim = user.FindFirst("tenant_code");
            return claim?.Value;
        }
    }
}
