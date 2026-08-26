using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Profile;

public static class UpdateProfile
{
    public record Command(string FullName, string? PhoneNumber) : IRequest<Response>;

    public record Response(Guid Id, string FullName, string? PhoneNumber);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
            RuleFor(x => x.PhoneNumber).MaximumLength(30);
        }
    }

    public class Handler(UserDbContext db, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken ct)
        {
            var userId = Guid.Parse(httpContextAccessor.HttpContext!.User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = await db.Users.FindAsync([userId], ct)
                ?? throw new InvalidOperationException("User not found.");

            user.FullName = request.FullName;
            user.PhoneNumber = request.PhoneNumber;
            user.SysUserModified = user.Email;
            user.SysDateModified = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);

            return new Response(user.Id, user.FullName, user.PhoneNumber);
        }
    }
}
