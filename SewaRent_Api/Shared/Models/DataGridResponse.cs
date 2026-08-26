namespace SewaRent_Api.Shared.Models;

public record DataGridResponse<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int TotalPages,
    int CurrentPage,
    int PageSize);
