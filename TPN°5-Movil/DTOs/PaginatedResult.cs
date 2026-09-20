using Microsoft.EntityFrameworkCore;

namespace TPN_5_MOVIL.DTOs;

public class PaginatedResult<T>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public bool HasNext { get; init; }
    public bool HasPrevious { get; init; }
    public List<T> Items { get; init; } = new();

    public static async Task<PaginatedResult<T>> CreateAsync(IQueryable<T> source, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var totalCount = await source.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await source
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<T>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNext = page < totalPages,
            HasPrevious = page > 1,
            Items = items
        };
    }
}