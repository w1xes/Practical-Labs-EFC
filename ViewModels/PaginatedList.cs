using Microsoft.EntityFrameworkCore;

namespace TaskEFC.ViewModels;

/// <summary>
/// One page of a query result.
/// </summary>
public class PaginatedList<T> : List<T>
{
    private PaginatedList(IEnumerable<T> items, int pageIndex, int totalPages)
        : base(items)
    {
        PageIndex = pageIndex;
        TotalPages = totalPages;
    }

    public int PageIndex { get; }

    public int TotalPages { get; }

    public bool HasPreviousPage => PageIndex > 1;

    public bool HasNextPage => PageIndex < TotalPages;

    /// <param name="source">An ordered query; paging over an unordered one is not deterministic.</param>
    /// <param name="pageIndex">One-based page number, clamped to the existing pages.</param>
    /// <param name="pageSize">Number of items per page.</param>
    public static async Task<PaginatedList<T>> CreateAsync(IOrderedQueryable<T> source, int pageIndex, int pageSize)
    {
        var count = await source.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(count / (double)pageSize));
        pageIndex = Math.Clamp(pageIndex, 1, totalPages);

        var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PaginatedList<T>(items, pageIndex, totalPages);
    }
}
