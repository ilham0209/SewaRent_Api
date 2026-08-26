using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Features.Auth;
using SewaRent_Api.Shared.Domain.User;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Tests.Features.Auth;

public class RegisterTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesUser()
    {
        await using var db = TestDbFactory.CreateUserDb();
        db.Roles.Add(new RoleEntity { Id = Guid.NewGuid(), Name = "Tenant", SysUserCreated = "System", SysDateCreated = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var handler = new Register.Handler(db);
        var command = new Register.Command("John Doe", "john@example.com", "password123", null, "Tenant");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("John Doe", result.FullName);
        Assert.Equal("john@example.com", result.Email);
        Assert.Equal("Tenant", result.Role);
        Assert.Single(db.Users);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsInvalidOperation()
    {
        await using var db = TestDbFactory.CreateUserDb();
        var role = new RoleEntity { Id = Guid.NewGuid(), Name = "Tenant", SysUserCreated = "System", SysDateCreated = DateTime.UtcNow };
        db.Roles.Add(role);
        db.Users.Add(new UserEntity
        {
            Email = "john@example.com",
            FullName = "Existing",
            PasswordHash = "hash",
            IsActive = true,
            SysUserCreated = "System",
            SysDateCreated = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new Register.Handler(db);
        var command = new Register.Command("Jane", "john@example.com", "password123", null, "Tenant");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InvalidRole_ThrowsInvalidOperation()
    {
        await using var db = TestDbFactory.CreateUserDb();
        var handler = new Register.Handler(db);
        var command = new Register.Command("John", "john@example.com", "password123", null, "SuperAdmin");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}
