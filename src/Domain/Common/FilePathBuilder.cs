using Bookazone.Domain.Enums;

namespace Bookazone.Domain.Common;

public static class FilePathBuilder
{
    public static string BuildTenantPath(Guid tenantId, FileCategory category)
    {
        return Path.Combine("storage", "tenants", tenantId.ToString(), category.ToString().ToLower());
    }
}
