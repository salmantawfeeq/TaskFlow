namespace TaskFlow.Application.Common;

/// <summary>
/// Generic wrapper for any paginated list result, so every service method
/// that returns a page of data (Projects, Tasks, Users, Reports...) shares
/// the same shape instead of each controller reinventing paging metadata.
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;
}
