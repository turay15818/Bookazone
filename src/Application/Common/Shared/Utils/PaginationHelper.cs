using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Bookazone.Application.Common.Shared.Utils;

public static class PaginationHelper
{
    public static async Task<PagedResult<T>> ToPagedListAsync<T>(
        this IQueryable<T> query,
        int pageIndex,
        int pageSize,
        string? search = null,
        Expression<Func<T, bool>>? searchPredicate = null,
        CancellationToken ct = default) where T : class
    {
        if (!string.IsNullOrWhiteSpace(search) && searchPredicate != null)
            query = query.Where(searchPredicate);

        var totalItems = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync(ct);

        return new PagedResult<T>(items, totalPages, totalItems, pageIndex, pageSize);
    }
}

public class PagedResult<T>(IReadOnlyList<T> items, int totalPages, int totalItems, int pageIndex, int pageSize)
{
    public IReadOnlyList<T> Items { get; } = items;
    public int TotalPages { get; } = totalPages;
    public int TotalItems { get; } = totalItems;
    public int PageIndex { get; } = pageIndex;
    public int PageSize { get; } = pageSize;
}