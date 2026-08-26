using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Domain.Property;

namespace SewaRent_Api.Shared.Infrastructure.Persistence;

public class PropertyDbContext(DbContextOptions<PropertyDbContext> options) : DbContext(options)
{
    public DbSet<PropertyEntity> Properties => Set<PropertyEntity>();
    public DbSet<PropertyTypeEntity> PropertyTypes => Set<PropertyTypeEntity>();
    public DbSet<PropertyImageEntity> PropertyImages => Set<PropertyImageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PropertyEntity>(e =>
        {
            e.ToTable("PR_Property");
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.MonthlyRent).HasColumnType("decimal(18,2)");
            e.Property(x => x.AddressLine1).IsRequired().HasMaxLength(255);
            e.Property(x => x.AddressLine2).HasMaxLength(255);
            e.Property(x => x.City).IsRequired().HasMaxLength(100);
            e.Property(x => x.State).IsRequired().HasMaxLength(100);
            e.Property(x => x.Postcode).HasMaxLength(20);
            e.Property(x => x.Latitude).HasColumnType("decimal(10,7)");
            e.Property(x => x.Longitude).HasColumnType("decimal(10,7)");
            e.Property(x => x.AvailabilityStatus).IsRequired().HasMaxLength(30);
            e.HasIndex(x => x.LandlordId);
            e.HasIndex(x => x.PropertyTypeId);
            e.HasIndex(x => x.City);
            e.HasIndex(x => x.State);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<PropertyTypeEntity>(e =>
        {
            e.ToTable("PR_PropertyTypes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Description).HasMaxLength(255);
            e.HasIndex(x => x.Name).IsUnique();
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<PropertyImageEntity>(e =>
        {
            e.ToTable("PR_PropertyImages");
            e.HasKey(x => x.Id);
            e.Property(x => x.ImageUrl).IsRequired().HasMaxLength(1000);
            e.HasIndex(x => x.PropertyId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });
    }
}
