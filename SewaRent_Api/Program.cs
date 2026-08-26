using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SewaRent_Api.Shared.Domain.Property;
using SewaRent_Api.Shared.Domain.RentalRequest;
using SewaRent_Api.Shared.Domain.User;
using SewaRent_Api.Shared.Infrastructure.Behavior;
using SewaRent_Api.Shared.Infrastructure.Persistence;
using SewaRent_Api.Shared.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi(options =>
{
    options.CreateSchemaReferenceId = jsonType =>
    {
        var type = jsonType.Type;
        return type.DeclaringType != null
            ? $"{type.DeclaringType.Name}{type.Name}"
            : Microsoft.AspNetCore.OpenApi.OpenApiOptions.CreateDefaultSchemaReferenceId(jsonType);
    };
}); builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<SewaRentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SewaRent")));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "SewaRent API";
        options.ShowSidebar = true;
    });
}

app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

ApplyMigration();
SeedRoles();
SeedPropertyTypes();
SeedRentalRequestStatuses();

app.Run();

void ApplyMigration()
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<SewaRentDbContext>();
    db.Database.Migrate();
}

void SeedRoles()
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<SewaRentDbContext>();

    if (!db.Roles.Any())
    {
        var roles = new[]
        {
            new RoleEntity { Name = "Tenant", Description = "Tenant role", SysUserCreated = "System", SysDateCreated = DateTime.UtcNow },
            new RoleEntity { Name = "Landlord", Description = "Landlord role", SysUserCreated = "System", SysDateCreated = DateTime.UtcNow },
            new RoleEntity { Name = "Admin", Description = "Administrator role", SysUserCreated = "System", SysDateCreated = DateTime.UtcNow }
        };

        db.Roles.AddRange(roles);
        db.SaveChanges();
    }
}

void SeedPropertyTypes()
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<SewaRentDbContext>();

    if (!db.PropertyTypes.Any())
    {
        var propertyTypes = new[]
        {
            new PropertyTypeEntity { Name = "Apartment", Description = "Unit in a multi-storey building", IsActive = true, SysUserCreated = "System", SysDateCreated = DateTime.UtcNow },
            new PropertyTypeEntity { Name = "Condo", Description = "Condominium with shared facilities", IsActive = true, SysUserCreated = "System", SysDateCreated = DateTime.UtcNow },
            new PropertyTypeEntity { Name = "Landed", Description = "Landed house or bungalow", IsActive = true, SysUserCreated = "System", SysDateCreated = DateTime.UtcNow },
            new PropertyTypeEntity { Name = "Room", Description = "Single room for rent", IsActive = true, SysUserCreated = "System", SysDateCreated = DateTime.UtcNow }
        };

        db.PropertyTypes.AddRange(propertyTypes);
        db.SaveChanges();
    }
}

void SeedRentalRequestStatuses()
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<SewaRentDbContext>();

    if (!db.RentalRequestStatuses.Any())
    {
        var statuses = new[]
        {
            new RentalRequestStatusEntity { Name = "Pending", Description = "Request is awaiting landlord action", SysUserCreated = "System", SysDateCreated = DateTime.UtcNow },
            new RentalRequestStatusEntity { Name = "Approved", Description = "Landlord has approved the request", SysUserCreated = "System", SysDateCreated = DateTime.UtcNow },
            new RentalRequestStatusEntity { Name = "Rejected", Description = "Landlord has rejected the request", SysUserCreated = "System", SysDateCreated = DateTime.UtcNow },
            new RentalRequestStatusEntity { Name = "Cancelled", Description = "Tenant has cancelled the request", SysUserCreated = "System", SysDateCreated = DateTime.UtcNow }
        };

        db.RentalRequestStatuses.AddRange(statuses);
        db.SaveChanges();
    }
}
