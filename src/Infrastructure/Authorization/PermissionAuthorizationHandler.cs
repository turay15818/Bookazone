using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Bookazone.Infrastructure.Services;

namespace Bookazone.Infrastructure.Authorization;

public class PermissionAuthorizationHandler(
    IPermissionService permissionService,
    ILogger<PermissionAuthorizationHandler> logger
) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement
    )
    {
        try
        {
            // Try to get user ID from multiple common claim types (NameIdentifier, sub)
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                              ?? context.User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                logger.LogWarning(
                    "No user ID claim found — permission check failed for {Permission}",
                    requirement.Permission
                );
                return;
            }

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                logger.LogWarning(
                    "Invalid user ID claim — cannot authorize permission {Permission}",
                    requirement.Permission
                );
                return;
            }

            var hasPermission = await permissionService.HasPermissionAsync(
                userId,
                requirement.Permission
            );
            if (hasPermission)
            {
                context.Succeed(requirement);
            }
            else
            {
                logger.LogInformation(
                    "User {UserId} denied for permission {Permission}",
                    userId,
                    requirement.Permission
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating permission {Permission}", requirement.Permission);
        }
    }
}
