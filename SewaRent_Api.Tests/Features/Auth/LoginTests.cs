using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SewaRent_Api.Features.Auth;
using SewaRent_Api.Shared.Domain.User;
using SewaRent_Api.Shared.Infrastructure.Persistence;
using SewaRent_Api.Shared.Utilities;

namespace SewaRent_Api.Tests.Features.Auth;

public class LoginTests
{
    [Fact]
    public async Task Handle_ValidCredentials_ReturnsToken()
    {
        await using var db = TestDbFactory.CreateUserDb();
        var role = new RoleEntity { Id = Guid.NewGuid(), Name = "Tenant", SysUserCreated = "System", SysDateCreated = DateTime.UtcNow };
        db.Roles.Add(role);

        var user = new UserEntity
        {
            Email = "john@example.com",
            FullName = "John Doe",
            PasswordHash = JwtHelper.HashPassword("password123"),
            IsActive = true,
            SysUserCreated = "System",
            SysDateCreated = DateTime.UtcNow
        };
        db.Users.Add(user);

        var userRole = new UserRoleEntity { UserId = user.Id, RoleId = role.Id, CreatedAt = DateTime.UtcNow };
        db.UserRoles.Add(userRole);
        await db.SaveChangesAsync();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "test_secret_key_that_is_long_enough_for_hmac256!" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:ExpiryInMinutes", "60" }
            })
            .Build();

        var handler = new Login.Handler(db, config);
        var command = new Login.Command("john@example.com", "password123");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result.AccessToken);
        Assert.Equal("John Doe", result.User.Name);
        Assert.Equal("Tenant", result.User.Role);
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsInvalidOperation()
    {
        await using var db = TestDbFactory.CreateUserDb();
        var role = new RoleEntity { Id = Guid.NewGuid(), Name = "Tenant", SysUserCreated = "System", SysDateCreated = DateTime.UtcNow };
        db.Roles.Add(role);

        var user = new UserEntity
        {
            Email = "john@example.com",
            FullName = "John Doe",
            PasswordHash = JwtHelper.HashPassword("password123"),
            IsActive = true,
            SysUserCreated = "System",
            SysDateCreated = DateTime.UtcNow
        };
        db.Users.Add(user);
        db.UserRoles.Add(new UserRoleEntity { UserId = user.Id, RoleId = role.Id, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "test_secret_key_that_is_long_enough_for_hmac256!" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:ExpiryInMinutes", "60" }
            })
            .Build();

        var handler = new Login.Handler(db, config);
        var command = new Login.Command("john@example.com", "wrongpassword");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistentEmail_ThrowsInvalidOperation()
    {
        await using var db = TestDbFactory.CreateUserDb();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "test_secret_key_that_is_long_enough_for_hmac256!" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:ExpiryInMinutes", "60" }
            })
            .Build();

        var handler = new Login.Handler(db, config);
        var command = new Login.Command("nonexistent@example.com", "password123");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}
