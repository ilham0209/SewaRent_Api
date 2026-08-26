using SewaRent_Api.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace SewaRent_Api.Shared.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, string? search, params string[] searchProperties)
    {
        if (string.IsNullOrWhiteSpace(search) || searchProperties.Length == 0)
            return query;

        var searchLower = search.ToLower();
        var paramName = "x";
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), paramName);

        System.Linq.Expressions.Expression? combined = null;
        foreach (var prop in searchProperties)
        {
            var propertyAccess = System.Linq.Expressions.Expression.Property(parameter, prop);
            var nullCheck = System.Linq.Expressions.Expression.NotEqual(
                propertyAccess, System.Linq.Expressions.Expression.Constant(null, typeof(string)));
            var toLower = System.Linq.Expressions.Expression.Call(
                propertyAccess, typeof(string).GetMethod("ToLower", Type.EmptyTypes)!);
            var contains = System.Linq.Expressions.Expression.Call(
                toLower, typeof(string).GetMethod("Contains", [typeof(string)])!,
                System.Linq.Expressions.Expression.Constant(searchLower));
            var expr = System.Linq.Expressions.Expression.AndAlso(nullCheck, contains);
            combined = combined is null ? expr : System.Linq.Expressions.Expression.OrElse(combined, expr);
        }

        if (combined is not null)
        {
            var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(combined, parameter);
            query = query.Where(lambda);
        }

        return query;
    }

    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int page, int pageSize)
    {
        return query.Skip((page - 1) * pageSize).Take(pageSize);
    }

    public static async Task<DataGridResponse<T>> ToDataGridResponseAsync<T>(
        this IQueryable<T> source,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var totalCount = await source.CountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var items = await source
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new DataGridResponse<T>(items, totalCount, totalPages, page, pageSize);
    }
}
