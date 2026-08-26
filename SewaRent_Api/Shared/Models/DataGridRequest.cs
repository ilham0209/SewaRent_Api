namespace SewaRent_Api.Shared.Models;

public record DataGridRequest(
    int Page = 1,
    int PageSize = 10,
    string? Search = null);
