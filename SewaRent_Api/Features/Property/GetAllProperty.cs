using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Extensions;
using SewaRent_Api.Shared.Infrastructure.Persistence;
using SewaRent_Api.Shared.Models;

namespace SewaRent_Api.Features.Property;

public static class GetAllProperty
{
    public record Query(
        int Page,
        int PageSize,
        string? Search,
        string? City,
        string? State,
        Guid? PropertyTypeId,
        decimal? MinRent,
        decimal? MaxRent,
        int? Bedrooms,
        bool? IsFurnished)
        : IRequest<DataGridResponse<PropertySummary>>;

    public record PropertySummary(
        Guid Id,
        string Title,
        decimal MonthlyRent,
        string City,
        string State,
        int Bedrooms,
        int Bathrooms,
        bool IsFurnished,
        string PropertyTypeName,
        string? ImageUrl);

    public class Handler(SewaRentDbContext db)
        : IRequestHandler<Query, DataGridResponse<PropertySummary>>
    {
        public async Task<DataGridResponse<PropertySummary>> Handle(Query request, CancellationToken ct)
        {
            var query = db.Properties
                .Include(p => p.PropertyType)
                .Include(p => p.PropertyImages)
                .Where(p => p.IsActive && !p.IsDeleted)
                .AsQueryable();

            query = query.ApplySearch(request.Search, nameof(Shared.Domain.Property.PropertyEntity.Title),
                nameof(Shared.Domain.Property.PropertyEntity.City));

            if (!string.IsNullOrWhiteSpace(request.City))
                query = query.Where(p => p.City == request.City);

            if (!string.IsNullOrWhiteSpace(request.State))
                query = query.Where(p => p.State == request.State);

            if (request.PropertyTypeId.HasValue)
                query = query.Where(p => p.PropertyTypeId == request.PropertyTypeId.Value);

            if (request.MinRent.HasValue)
                query = query.Where(p => p.MonthlyRent >= request.MinRent.Value);

            if (request.MaxRent.HasValue)
                query = query.Where(p => p.MonthlyRent <= request.MaxRent.Value);

            if (request.Bedrooms.HasValue)
                query = query.Where(p => p.Bedrooms == request.Bedrooms.Value);

            if (request.IsFurnished.HasValue)
                query = query.Where(p => p.IsFurnished == request.IsFurnished.Value);

            var projectedQuery = query.Select(p => new PropertySummary(
                p.Id,
                p.Title,
                p.MonthlyRent,
                p.City,
                p.State,
                p.Bedrooms,
                p.Bathrooms,
                p.IsFurnished,
                p.PropertyType.Name,
                p.PropertyImages
                    .OrderByDescending(i => i.IsPrimary)
                    .ThenBy(i => i.SortOrder)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault()));

            return await projectedQuery.ToDataGridResponseAsync(request.Page, request.PageSize, ct);
        }
    }
}
