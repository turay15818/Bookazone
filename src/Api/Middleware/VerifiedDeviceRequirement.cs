using Microsoft.AspNetCore.Authorization;

namespace Bookazone.Api.Middleware;

public class VerifiedDeviceRequirement : IAuthorizationRequirement
{
}