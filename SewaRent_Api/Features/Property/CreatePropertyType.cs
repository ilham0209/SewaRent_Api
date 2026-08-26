using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Domain.Property;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Property;

public static class CreatePropertyType
{
    public record Command(
        string Name,
        string? Description) : IRequest<Response>;

    public record Response(Guid Id, string Name, string? Description);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).MaximumLength(255);
        }
    }

    public class Handler(SewaRentDbContext db, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken ct)
        {
            var role = httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.Role);

            if (role != "Admin")
                throw new UnauthorizedAccessException("Only admins can create property types.");

            var exists = await db.PropertyTypes
                .AnyAsync(x => x.Name == request.Name, ct);

            if (exists)
                throw new InvalidOperationException("Property type already exists.");

            var propertyType = new PropertyTypeEntity
            {
                Name = request.Name,
                Description = request.Description,
                IsActive = true,
                SysUserCreated = httpContextAccessor.HttpContext!.User
                    .FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                SysDateCreated = DateTime.UtcNow
            };

            db.PropertyTypes.Add(propertyType);
            await db.SaveChangesAsync(ct);

            return new Response(propertyType.Id, propertyType.Name, propertyType.Description);
        }
    }
}
