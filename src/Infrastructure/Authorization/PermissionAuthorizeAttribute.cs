using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Bookazone.Application.Common.Shared.Utils;


namespace Bookazone.Infrastructure.Authorization;

public class PermissionAuthorizeAttribute(string requiredPermission) : AuthorizeAttribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user?.Identity == null || !user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        // Note: Do not treat role values as permissions directly; only expand roles to permissions via mapping
        // to avoid accidentally authorizing when role name matches a permission name.
        foreach (var _ in user.FindAll(ClaimTypes.Role))
        {
            // Intentionally not adding raw role values into permissions set
        }
        foreach (var p in user.FindAll("permission"))
        {
            if (!string.IsNullOrWhiteSpace(p.Value))
                permissions.Add(p.Value);
        }
        foreach (var roleValue in user.FindAll(ClaimTypes.Role).Select(c => c.Value))
        {
            var expanded = GetPermissionsForRole(roleValue);
            if (expanded is { Count: > 0 })
            {
                foreach (var perm in expanded)
                    permissions.Add(perm);
            }
        }

        if (!permissions.Contains(requiredPermission))
        {
            context.Result = new ForbidResult();
            return;
        }
        await Task.CompletedTask;
    }

    private List<string> GetPermissionsForRole(string role)
    {
        return role switch
        {
            Consts.Roles.PlatformSuperAdmin => Consts.RolePermissions.PlatformSuperAdmin,
            Consts.Roles.TenantAdmin => Consts.RolePermissions.TenantAdmin,
            Consts.Roles.TenantManager => Consts.RolePermissions.TenantManager,
            Consts.Roles.TenantFinance => Consts.RolePermissions.TenantFinance,
            _ => new List<string>()
        };
    }
}
