using Microsoft.AspNetCore.Authorization;

namespace Bookazone.Infrastructure.Authorization;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class HasPermissionAttribute(string permission) : AuthorizeAttribute(policy: permission);