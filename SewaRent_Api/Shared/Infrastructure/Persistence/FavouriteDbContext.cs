using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Domain.Favourite;

namespace SewaRent_Api.Shared.Infrastructure.Persistence;

public class FavouriteDbContext(DbContextOptions<FavouriteDbContext> options) : DbContext(options)
{
    public DbSet<FavouriteEntity> Favourites => Set<FavouriteEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FavouriteEntity>(e =>
        {
            e.ToTable("FA_Favourites");
            e.HasKey(x => new { x.UserId, x.PropertyId });
        });
    }
}
