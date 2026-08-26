using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Domain.Property;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Property;

public static class UploadPropertyImage
{
    public record Command(Guid PropertyId, string ImageUrl, bool IsPrimary, int SortOrder) : IRequest<Response>;

    public record Response(Guid Id, string ImageUrl, bool IsPrimary, int SortOrder);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.PropertyId).NotEmpty();
            RuleFor(x => x.ImageUrl).NotEmpty().MaximumLength(1000);
            RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
        }
    }

    public class Handler(SewaRentDbContext db, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken ct)
        {
            var userId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var property = await db.Properties.FindAsync([request.PropertyId], ct)
                ?? throw new InvalidOperationException("Property not found.");

            if (property.LandlordId != userId)
                throw new UnauthorizedAccessException("You can only add images to your own properties.");

            if (request.IsPrimary)
            {
                var existingPrimary = await db.PropertyImages
                    .Where(i => i.PropertyId == request.PropertyId && i.IsPrimary)
                    .ToListAsync(ct);

                foreach (var img in existingPrimary)
                    img.IsPrimary = false;
            }

            var image = new PropertyImageEntity
            {
                PropertyId = request.PropertyId,
                ImageUrl = request.ImageUrl,
                IsPrimary = request.IsPrimary,
                SortOrder = request.SortOrder,
                SysUserCreated = httpContextAccessor.HttpContext!.User
                    .FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                SysDateCreated = DateTime.UtcNow
            };

            db.PropertyImages.Add(image);
            await db.SaveChangesAsync(ct);

            return new Response(image.Id, image.ImageUrl, image.IsPrimary, image.SortOrder);
        }
    }
}
