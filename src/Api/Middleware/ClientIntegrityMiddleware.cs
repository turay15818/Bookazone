namespace Bookazone.Api.Middleware;

public class ClientIntegrityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ClientIntegrityMiddleware> _logger;

    public ClientIntegrityMiddleware(RequestDelegate next, ILogger<ClientIntegrityMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        var claims = context.User;

        if (claims?.Identity?.IsAuthenticated == true)
        {
            var appType = claims.FindFirst("app_type")?.Value;

            if (appType == "web" && !userAgent.Contains("Mozilla", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Rejected suspicious token use: non-browser client for web token");
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Suspicious token usage detected.");
                return;
            }
        }

        await _next(context);
    }
}
