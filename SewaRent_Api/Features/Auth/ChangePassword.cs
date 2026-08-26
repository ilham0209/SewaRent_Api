using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;
using SewaRent_Api.Shared.Utilities;

namespace SewaRent_Api.Features.Auth;

public static class ChangePassword
{
    public record Command(string CurrentPassword, string NewPassword) : IRequest<Response>;

    public record Response(bool Success);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.CurrentPassword).NotEmpty();
            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
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

            if (!JwtHelper.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                throw new InvalidOperationException("Current password is incorrect.");

            user.PasswordHash = JwtHelper.HashPassword(request.NewPassword);
            user.SysUserModified = user.Email;
            user.SysDateModified = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);

            return new Response(true);
        }
    }
}
