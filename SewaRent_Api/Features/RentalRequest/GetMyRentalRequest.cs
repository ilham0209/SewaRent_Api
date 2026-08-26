using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;
using SewaRent_Api.Shared.Models;

namespace SewaRent_Api.Features.RentalRequest;

public static class GetMyRentalRequest
{
    public record Query(int Page, int PageSize) : IRequest<DataGridResponse<RentalRequestSummary>>;

    public record RentalRequestSummary(
        Guid Id,
        string PropertyTitle,
        string PropertyCity,
        string Status,
        DateTime RequestedAt,
        DateTime? DecisionAt);

    public class Handler(RentalRequestDbContext rentalDb, PropertyDbContext propertyDb,
        IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Query, DataGridResponse<RentalRequestSummary>>
    {
        public async Task<DataGridResponse<RentalRequestSummary>> Handle(Query request, CancellationToken ct)
        {
            var tenantId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var query = from rr in rentalDb.RentalRequests
                        join p in propertyDb.Properties on rr.PropertyId equals p.Id
                        join s in rentalDb.RentalRequestStatuses on rr.StatusId equals s.Id
                        where rr.TenantId == tenantId && !rr.IsDeleted
                        orderby rr.RequestedAt descending
                        select new RentalRequestSummary(
                            rr.Id,
                            p.Title,
                            p.City,
                            s.Name,
                            rr.RequestedAt,
                            rr.DecisionAt);

            var totalCount = await query.CountAsync(ct);
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            return new DataGridResponse<RentalRequestSummary>(
                items, totalCount, totalPages, request.Page, request.PageSize);
        }
    }
}
