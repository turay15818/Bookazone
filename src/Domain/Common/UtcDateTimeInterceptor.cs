using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Bookazone.Domain.Common;

public sealed class UtcDateTimeInterceptor : SaveChangesInterceptor
{
    private static void Normalize(EntityEntry entry)
    {
        foreach (var property in entry.Properties)
        {
            if (property.Metadata.ClrType == typeof(DateTime) &&
                property.CurrentValue is DateTime dt &&
                dt.Kind != DateTimeKind.Utc)
            {
                property.CurrentValue = DateTime.SpecifyKind(
                    dt.ToUniversalTime(),
                    DateTimeKind.Utc);
            }
        }
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var context = eventData.Context;
        if (context == null) return result;
        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                Normalize(entry);
            }
        }
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        SavingChanges(eventData, result);
        return new(result);
    }
}
