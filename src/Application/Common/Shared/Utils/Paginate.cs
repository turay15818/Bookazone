using Microsoft.EntityFrameworkCore;
using Bookazone.Application.DTOs;

namespace Bookazone.Application.Common.Shared.Utils;


public class Paginate<T> : DataPaginate<T>
{

    private Paginate(IEnumerable<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        Pages = (int)Math.Ceiling(count / (double)pageSize);

        Data.AddRange(items);
    }

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < Pages;

    public static async Task<Paginate<T>> CreateAsync(IQueryable<T> source, int? pageIndex, int? pageSize)
    {
        var pIndex = pageIndex is > 0 ? (int)pageIndex : 1;
        var pSize = pageSize is > 0 ? (int)pageSize : 100;
        var count = await source.CountAsync();
        var items = await source.Skip((pIndex - 1) * pSize).Take(pSize).ToListAsync();
        return new Paginate<T>(items, count, pIndex, pSize);
    }
}

