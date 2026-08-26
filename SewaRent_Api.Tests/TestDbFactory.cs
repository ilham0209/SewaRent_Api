using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Tests;

public static class TestDbFactory
{
    public static UserDbContext CreateUserDb()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new UserDbContext(options);
    }

    public static PropertyDbContext CreatePropertyDb()
    {
        var options = new DbContextOptionsBuilder<PropertyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PropertyDbContext(options);
    }

    public static RentalRequestDbContext CreateRentalRequestDb()
    {
        var options = new DbContextOptionsBuilder<RentalRequestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new RentalRequestDbContext(options);
    }

    public static FavouriteDbContext CreateFavouriteDb()
    {
        var options = new DbContextOptionsBuilder<FavouriteDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new FavouriteDbContext(options);
    }

    public static IHttpContextAccessor CreateHttpContextAccessor(Guid userId, string email)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var context = new DefaultHttpContext { User = principal };

        var accessor = new HttpContextAccessor();
        accessor.HttpContext = context;
        return accessor;
    }
}
