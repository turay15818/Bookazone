using Microsoft.AspNetCore.Http;

namespace Bookazone.Application.Common.Shared.Utils;

public static class FileUrlHelper
{
    public static string? BuildPublicUrl(HttpRequest? request, string? path, string? baseUrlOverride = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        if (Uri.TryCreate(path, UriKind.Absolute, out _))
            return path;

        var normalized = path.Replace("\\", "/");
        if (!normalized.StartsWith('/'))
            normalized = "/" + normalized;

        var baseUrl = string.IsNullOrWhiteSpace(baseUrlOverride)
            ? BuildBaseUrl(request)
            : baseUrlOverride;

        if (string.IsNullOrWhiteSpace(baseUrl))
            return normalized;

        return $"{baseUrl.TrimEnd('/')}{normalized}";
    }

    private static string? BuildBaseUrl(HttpRequest? request)
    {
        if (request == null)
            return null;

        return $"{request.Scheme}://{request.Host}{request.PathBase}";
    }
}
