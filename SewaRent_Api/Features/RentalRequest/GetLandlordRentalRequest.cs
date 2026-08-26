using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;
using SewaRent_Api.Shared.Models;

namespace SewaRent_Api.Features.RentalRequest;

public static class GetLandlordRentalRequest
{
    public record Query(int Page, int PageSize, Guid? PropertyId) : IRequest<DataGridResponse<LandlordRequestSummary>>;

    public record LandlordRequestSummary(
        Guid Id,
        Guid PropertyId,
        string PropertyTitle,
        Guid TenantId,
        string TenantName,
        string Status,
        DateTime RequestedAt);

    public class Handler(RentalRequestDbContext rentalDb, PropertyDbContext propertyDb,
        UserDbContext userDb, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Query, DataGridResponse<LandlordRequestSummary>>
    {
        public async Task<DataGridResponse<LandlordRequestSummary>> Handle(Query request, CancellationToken ct)
        {
            var landlordId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var landlordPropertyIds = await propertyDb.Properties
                .Where(p => p.LandlordId == landlordId && !p.IsDeleted)
                .Select(p => p.Id)
                .ToListAsync(ct);

            var query = from rr in rentalDb.RentalRequests
                        join p in propertyDb.Properties on rr.PropertyId equals p.Id
                        join u in userDb.Users on rr.TenantId equals u.Id
                        join s in rentalDb.RentalRequestStatuses on rr.StatusId equals s.Id
                        where landlordPropertyIds.Contains(rr.PropertyId) && !rr.IsDeleted
                        orderby rr.RequestedAt descending
                        select new LandlordRequestSummary(
                            rr.Id,
                            rr.PropertyId,
                            p.Title,
                            rr.TenantId,
                            u.FullName,
                            s.Name,
                            rr.RequestedAt);

            if (request.PropertyId.HasValue)
                query = query.Where(x => x.PropertyId == request.PropertyId.Value);

            var totalCount = await query.CountAsync(ct);
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            return new DataGridResponse<LandlordRequestSummary>(
                items, totalCount, totalPages, request.Page, request.PageSize);
        }
    }
}
