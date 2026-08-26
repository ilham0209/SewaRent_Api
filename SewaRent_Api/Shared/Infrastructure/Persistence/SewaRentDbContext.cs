using Microsoft.EntityFrameworkCore;
using SewaRent_Api.Shared.Domain.Favourite;
using SewaRent_Api.Shared.Domain.Property;
using SewaRent_Api.Shared.Domain.RentalRequest;
using SewaRent_Api.Shared.Domain.User;

namespace SewaRent_Api.Shared.Infrastructure.Persistence;

public class SewaRentDbContext(DbContextOptions<SewaRentDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<RoleEntity> Roles => Set<RoleEntity>();
    public DbSet<UserRoleEntity> UserRoles => Set<UserRoleEntity>();
    public DbSet<PropertyEntity> Properties => Set<PropertyEntity>();
    public DbSet<PropertyTypeEntity> PropertyTypes => Set<PropertyTypeEntity>();
    public DbSet<PropertyImageEntity> PropertyImages => Set<PropertyImageEntity>();
    public DbSet<FavouriteEntity> Favourites => Set<FavouriteEntity>();
    public DbSet<RentalRequestEntity> RentalRequests => Set<RentalRequestEntity>();
    public DbSet<RentalRequestStatusEntity> RentalRequestStatuses => Set<RentalRequestStatusEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(e =>
        {
            e.ToTable("US_Users");
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).IsRequired().HasMaxLength(150);
            e.Property(x => x.Email).IsRequired().HasMaxLength(255);
            e.Property(x => x.PasswordHash).IsRequired().HasMaxLength(500);
            e.Property(x => x.PhoneNumber).HasMaxLength(30);
            e.Property(x => x.ProfileImageUrl).HasMaxLength(1000);
            e.HasIndex(x => x.Email).IsUnique();
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<RoleEntity>(e =>
        {
            e.ToTable("US_Roles");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(50);
            e.Property(x => x.Description).HasMaxLength(255);
            e.HasIndex(x => x.Name).IsUnique();
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<UserRoleEntity>(e =>
        {
            e.ToTable("US_UserRoles");
            e.HasKey(x => new { x.UserId, x.RoleId });
            e.HasOne(x => x.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(x => x.RoleId);
        });

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

        modelBuilder.Entity<FavouriteEntity>(e =>
        {
            e.ToTable("FA_Favourites");
            e.HasKey(x => new { x.UserId, x.PropertyId });
        });

        modelBuilder.Entity<RentalRequestEntity>(e =>
        {
            e.ToTable("RR_RentalRequests");
            e.HasKey(x => x.Id);
            e.Property(x => x.Message).HasMaxLength(1000);
            e.Property(x => x.DecisionNote).HasMaxLength(1000);
            e.HasIndex(x => x.TenantId);
            e.HasIndex(x => x.PropertyId);
            e.HasIndex(x => x.StatusId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<RentalRequestStatusEntity>(e =>
        {
            e.ToTable("RR_RentalRequestStatuses");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(50);
            e.Property(x => x.Description).HasMaxLength(255);
            e.HasIndex(x => x.Name).IsUnique();
            e.HasQueryFilter(x => !x.IsDeleted);
        });
    }
}
