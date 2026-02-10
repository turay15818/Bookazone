using Microsoft.AspNetCore.Authorization;
using Bookazone.Application.Interfaces.Repository.Profile;

namespace Bookazone.Api.Middleware;

public class VerifiedDeviceHandler(IDeviceRepository deviceRepository) : AuthorizationHandler<VerifiedDeviceRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, VerifiedDeviceRequirement requirement)
    {
        var deviceIdClaim = context.User.FindFirst("device_id")?.Value;
        var deviceVerifiedClaim = context.User.FindFirst("device_verified")?.Value;
        var purpose = context.User.FindFirst("purpose")?.Value;
        if (string.IsNullOrEmpty(deviceIdClaim) ||
            string.IsNullOrEmpty(deviceVerifiedClaim) ||
            deviceVerifiedClaim != "true" ||
            purpose != "session")
        {
            return Task.CompletedTask;
        }

        if (!Guid.TryParse(deviceIdClaim, out var deviceId))
        {
            return Task.CompletedTask;
        }
        var device = deviceRepository.Find(deviceId);
        if (device == null || device.Verified != true || device.Active != true)
        {
            return Task.CompletedTask;
        }
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}