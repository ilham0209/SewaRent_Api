# SewaRent API — Coding Style & Clean Code Guidelines

> **Purpose:** This document defines the coding style, structure, naming, commenting, and clean-code rules for the SewaRent API backend.
>
> The goal is to keep the C#/.NET code consistent, readable, maintainable, and easy for both developers and AI coding agents to understand.
>
> This document follows the `imas-dotnet-architecture` convention: vertical slice per feature, thin controllers, MediatR request pipeline, FluentValidation, EF Core code-first, and one `DbContext` per data domain.

---

## 1. Core Principles

SewaRent API code should follow these principles:

1. **Keep code simple.**
2. **Prefer readable code over clever code.**
3. **Keep each class/handler responsible for one clear operation.**
4. **Keep controllers focused on HTTP concerns only.**
5. **Keep business logic inside feature handlers, never in controllers or entities.**
6. **Do not duplicate logic unnecessarily.**
7. **Avoid premature abstraction.**
8. **Prefer existing project patterns over introducing new patterns.**
9. **Keep comments minimal and useful.**
10. **Do not write comments that simply repeat the code.**
11. **Use meaningful names instead of explanatory comments.**
12. **Follow the existing project structure before creating a new folder or abstraction.**

---

# 2. C# Formatting

Use the standard .NET formatter.

Run:

```bash
dotnet format
```

The project should rely on formatter output instead of manually formatting files.

Do not create custom formatting conventions that conflict with the .NET formatter or `.editorconfig`.

Enable `Nullable` and `ImplicitUsings` project-wide — do not disable them per file.

---

# 3. File Naming

Use `PascalCase` for all `.cs` filenames. No abbreviations.

Correct:

```text
PropertyController.cs
CreateProperty.cs
PropertyEntities.cs
PropertyDbContext.cs
GetByIdRentalRequest.cs
```

Incorrect:

```text
propertyController.cs
property_controller.cs
propCtrl.cs
```

The filename must match the primary artifact it represents — a feature file is named after the operation (`{Verb}{Domain}.cs`), not after the class inside it.

---

# 4. Class, Record, and File Naming Patterns

| Artifact | Pattern | Example |
|---|---|---|
| Feature file | `{Verb}{Domain}.cs` | `CreateProperty.cs` |
| Command/Query class | `{Verb}{Domain}Command` / `{Verb}{Domain}Query` | `CreatePropertyCommand` |
| Handler class | `{Verb}{Domain}Handler` | `CreatePropertyHandler` |
| Validator class | `{Verb}{Domain}Validator` | `CreatePropertyValidator` |
| Response DTO | `{Verb}{Domain}Response` | `CreatePropertyResponse` |
| Entity | `{Domain}Entity` | `PropertyEntity` |
| Entity file | `{Domain}Entities.cs` | `PropertyEntities.cs` |
| DbContext | `{Domain}DbContext` | `PropertyDbContext` |
| Controller | `{Domain}Controller` | `PropertyController` |
| Test class | `{Verb}{Domain}Tests` | `CreatePropertyTests` |

Do not use:

```csharp
class propertyController {}
class Property_Controller {}
```

---

# 5. Method and Variable Naming

Use `PascalCase` for methods, `camelCase` for local variables and parameters.

```csharp
public async Task<IActionResult> GetAllAsync() {}

var property = new PropertyEntity();
var rentalRequest = await db.RentalRequests.FindAsync(id);
```

Avoid unclear names:

```csharp
var x = ...
var d = ...
var temp = ...
```

Prefer:

```csharp
var property = ...
var propertyDetails = ...
var rentalRequest = ...
```

Short names are acceptable for very small scopes:

```csharp
foreach (var item in items) {}
```

---

# 6. Constants

Use constants when a value is genuinely constant, reused, or represents a meaningful application value.

```csharp
public const int DefaultPageSize = 20;
public const int MaxPropertyImages = 10;
```

Avoid scattering magic numbers:

```csharp
if (items.Count > 20) {}
```

Prefer:

```csharp
if (items.Count > DefaultPageSize) {}
```

Do not create constants for values only used once with no meaningful name.

---

# 7. Comments

## 7.1 General Rule

**Comments should be minimal.**

Do not comment every line.

Bad:

```csharp
// Get the property
var property = await db.Properties.FindAsync(id);

// Set the property title
var title = property.Title;

// Return the title
return title;
```

Prefer:

```csharp
var property = await db.Properties.FindAsync(id);
return property.Title;
```

## 7.2 Comment Only When Necessary

A comment is appropriate when it explains:

- Why something is required
- A non-obvious business rule
- A workaround
- An external limitation (e.g. an EF Core / SQL Server quirk)
- A security consideration
- A temporary implementation decision

Example:

```csharp
// RentalRequests must be excluded from cancellation once approved by the landlord.
if (request.StatusId == RentalRequestStatus.Approved)
{
    throw new InvalidOperationException("Approved requests cannot be cancelled.");
}
```

## 7.3 Avoid Comments That Repeat Code

Bad:

```csharp
// Mark property as inactive
property.IsActive = false;
```

Good:

```csharp
property.IsActive = false;
```

## 7.4 TODO Comments

Use TODO only when the task is genuinely incomplete.

```csharp
// TODO: Add rate limiting once the auth module is finalized.
```

Do not leave vague TODOs:

```csharp
// TODO: fix this
```

## 7.5 Comment Language

Use English for code comments. Keep comments short.

Architecture explanations belong in:

```text
README.md
INTEGRATION.md
DATABASE.md
CODING_STYLE.md
```

rather than inside source files.

---

# 8. Usings

Keep usings organized and let the IDE/formatter sort them.

```csharp
using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using SewaRent_Api.Shared.Domain.Property;
using SewaRent_Api.Shared.Infrastructure.Persistence;
```

Separate framework/package usings from project usings where practical (the formatter handles this via `.editorconfig`).

Remove unused usings. With `ImplicitUsings` enabled, do not re-add common BCL usings manually.

---

# 9. Nullable Reference Types

`Nullable` is enabled project-wide. Use it properly.

Prefer:

```csharp
public string? Description { get; set; }
```

when a value can genuinely be null (e.g. `Property.Description`, `User.PhoneNumber`).

Avoid unnecessary nullable values:

```csharp
public string? Title { get; set; }
```

if the domain guarantees `Title` always exists — use `public string Title { get; set; } = string.Empty;` instead.

Do not use the null-forgiving operator (`!`) casually.

Avoid:

```csharp
var name = property!.Title!;
```

Prefer proper handling:

```csharp
if (property is null)
{
    return NotFound();
}

var title = property.Title;
```

Use `!` only when the non-null state is guaranteed by program logic and the guarantee is clear from context.

---

# 10. Vertical Slice Feature Files

Each feature is **self-contained**: Command/Query + Validator + Response DTO + Handler all live in one `.cs` file under `Features/{Domain}/`.

```csharp
// Features/Property/CreateProperty.cs
using FluentValidation;
using MediatR;
using SewaRent_Api.Shared.Domain.Property;
using SewaRent_Api.Shared.Infrastructure.Persistence;

namespace SewaRent_Api.Features.Property;

// ── Command ──────────────────────────────────────────────
public record CreatePropertyCommand(
    Guid LandlordId,
    string Title,
    string? Description,
    decimal MonthlyRent,
    Guid PropertyTypeId) : IRequest<CreatePropertyResponse>;

// ── Validator ─────────────────────────────────────────────
public class CreatePropertyValidator : AbstractValidator<CreatePropertyCommand>
{
    public CreatePropertyValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MonthlyRent).GreaterThan(0);
        RuleFor(x => x.PropertyTypeId).NotEmpty();
    }
}

// ── Response ──────────────────────────────────────────────
public record CreatePropertyResponse(Guid Id, string Title, decimal MonthlyRent);

// ── Handler ───────────────────────────────────────────────
public class CreatePropertyHandler(PropertyDbContext db)
    : IRequestHandler<CreatePropertyCommand, CreatePropertyResponse>
{
    public async Task<CreatePropertyResponse> Handle(CreatePropertyCommand request, CancellationToken ct)
    {
        var property = new PropertyEntity
        {
            LandlordId = request.LandlordId,
            Title = request.Title,
            Description = request.Description,
            MonthlyRent = request.MonthlyRent,
            PropertyTypeId = request.PropertyTypeId
        };

        db.Properties.Add(property);
        await db.SaveChangesAsync(ct);

        return new CreatePropertyResponse(property.Id, property.Title, property.MonthlyRent);
    }
}
```

For **Query** features (`GetAll`, `GetById`), use `IRequest<T>` with a Query record. Use `DataGridRequest` / `DataGridResponse` for paginated list endpoints (e.g. `GetAllProperty`, `GetMyRentalRequest`).

No cross-feature dependencies. Code shared across features belongs in `Shared/`.

---

# 11. Controllers

Controllers are thin. No business logic — only dispatch to MediatR and return HTTP results.

```csharp
// Controllers/Property/PropertyController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SewaRent_Api.Features.Property;

namespace SewaRent_Api.Controllers.Property;

[ApiController]
[Route("api/properties")]
public class PropertyController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreatePropertyCommand command, CancellationToken ct)
        => Ok(await sender.Send(command, ct));

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetAllPropertyQuery query, CancellationToken ct)
        => Ok(await sender.Send(query, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetByIdPropertyQuery(id), ct));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdatePropertyCommand command, CancellationToken ct)
        => Ok(await sender.Send(command with { Id = id }, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeletePropertyCommand(id), ct);
        return NoContent();
    }
}
```

Never put EF Core queries, business rules, or manual mapping logic inside a controller action.

Bad:

```csharp
[HttpGet("{id:guid}")]
public async Task<IActionResult> GetById(Guid id)
{
    var property = await db.Properties.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
    if (property is null) return NotFound();
    return Ok(new { property.Title, property.MonthlyRent });
}
```

Prefer:

```csharp
[HttpGet("{id:guid}")]
public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    => Ok(await sender.Send(new GetByIdPropertyQuery(id), ct));
```

---

# 12. Business Logic Ownership

Business rules live in the feature **Handler**, never in the controller, entity, or DbContext.

Bad — rule enforced in the controller:

```csharp
[HttpPost("{id:guid}/approve")]
public async Task<IActionResult> Approve(Guid id, [FromServices] RentalRequestDbContext db)
{
    var request = await db.RentalRequests.FindAsync(id);
    if (request!.LandlordId != CurrentUserId) return Forbid(); // business rule leaking into HTTP layer
    request.StatusId = RentalRequestStatus.Approved;
    await db.SaveChangesAsync();
    return Ok();
}
```

Prefer: the ownership check and status transition live inside `ApproveRentalRequestHandler`, and the controller only sends the command.

---

# 13. Feature-First Structure

Business functionality belongs inside its domain folder, mirrored across `Controllers/`, `Features/`, and `Shared/Domain/`.

```text
Controllers/Property/
Features/Property/
Shared/Domain/Property/
```

Do not scatter one domain's files across unrelated folders. Do not create every subfolder prematurely — add structure when the domain actually needs it (see `README.md` §6 for the current SewaRent domain layout).

---

# 14. Entities & `BaseClass`

Every entity with a single surrogate key inherits `BaseClass` (`Shared/Domain/BaseClass.cs`) instead of redeclaring audit/soft-delete columns.

```csharp
// Shared/Domain/Property/PropertyEntities.cs
namespace SewaRent_Api.Shared.Domain.Property;

public class PropertyEntity : BaseClass
{
    public Guid LandlordId { get; set; }
    public Guid PropertyTypeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MonthlyRent { get; set; }
    public bool IsActive { get; set; }
}
```

Composite-key junction tables (e.g. `Favourites`, `UserRoles`) do **not** inherit `BaseClass` — see `DATABASE.md` §3.3. Keep entities as plain POCOs; `HasMaxLength` and other constraints belong in `OnModelCreating`, not on the entity class (see `DATABASE.md` §30).

---

# 15. DbContext & Migrations

Each data domain gets its **own** `DbContext` and its **own** migrations folder. Never share a `DbContext` across domains.

```csharp
// Shared/Infrastructure/Persistence/PropertyDbContext.cs
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
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.HasQueryFilter(x => !x.IsDeleted);
        });
    }
}
```

Add a global query filter (`HasQueryFilter(x => !x.IsDeleted)`) on every `BaseClass`-derived entity so soft-deleted rows are excluded automatically — do not rely on per-query `Where(x => !x.IsDeleted)` calls.

Create migrations per domain:

```text
Add-Migration -Name {Name} -Context PropertyDbContext
```

Migration files land in `Shared/Infrastructure/Migrations/PropertyDb/`.

---

# 16. Validation

FluentValidation is wired into the MediatR pipeline. Every Command that mutates state **must** have a corresponding `AbstractValidator<T>` in the same feature file.

Query handlers that only read data may omit a validator unless input sanitization is needed (e.g. page size limits).

Do not duplicate validation logic in the controller — validation belongs to the Command/Query in `Features/`.

---

# 17. MediatR Pipeline

Handlers use primary constructor injection (C# 12 style):

```csharp
public class GetByIdPropertyHandler(PropertyDbContext db) : IRequestHandler<GetByIdPropertyQuery, PropertyDetailsResponse>
```

Do not resolve `IServiceProvider` manually inside a handler — inject the specific dependency needed (DbContext, `IHttpContextAccessor` for the current user, etc.).

---

# 18. Pagination

List endpoints use `DataGridRequest` / `DataGridResponse` from `Shared/Models/`:

```csharp
public record GetAllPropertyQuery(int Page, int PageSize, string? Search)
    : DataGridRequest(Page, PageSize, Search), IRequest<DataGridResponse<PropertySummary>>;
```

Use `QueryableExtensions` (in `Shared/Extensions/`) for applying paging, search, and sorting to `IQueryable<T>` instead of writing manual `.Skip()/.Take()` in every handler.

---

# 19. Records vs. Classes

Use `record` for Commands, Queries, and Response DTOs — they are immutable data carriers.

```csharp
public record CreatePropertyCommand(string Title, decimal MonthlyRent) : IRequest<CreatePropertyResponse>;
```

Use `class` for EF Core entities (EF Core requires mutable, trackable reference types) and for Handlers/Validators.

---

# 20. Async/Await

All I/O (EF Core queries, HTTP calls, file uploads) must be `async` and pass `CancellationToken` through to `SaveChangesAsync`, `ToListAsync`, etc.

```csharp
public async Task<PropertyDetailsResponse> Handle(GetByIdPropertyQuery request, CancellationToken ct)
{
    var property = await db.Properties
        .FirstOrDefaultAsync(x => x.Id == request.Id, ct);
    ...
}
```

Never use `.Result` or `.Wait()` — this can deadlock in ASP.NET Core request pipelines.

---

# 21. Magic Strings & Enums

Avoid repeating important business strings throughout the application.

Bad:

```csharp
if (request.Status == "Approved") {}
```

Prefer an enum or a status entity reference:

```csharp
if (request.StatusId == RentalRequestStatus.Approved) {}
```

Use enums (or a lookup table, per `DATABASE.md` — e.g. `RentalRequestStatuses`) for finite states such as rental request status or property availability.

---

# 22. Dependency Injection

Register services centrally in `Program.cs`. Do not instantiate infrastructure dependencies (DbContext, HttpClient, etc.) directly inside a handler or controller.

Avoid:

```csharp
var db = new PropertyDbContext(new DbContextOptions<PropertyDbContext>());
```

Prefer constructor injection, configured once in `Program.cs`.

---

# 23. Security

Never store or expose:

```text
MSSQL connection strings in source control
JWT signing secrets in source control
Password hashes in API responses
Client-supplied UserId / LandlordId used for ownership checks
```

Always derive the current user's identity from the authenticated JWT (`ClaimsPrincipal`), never from a request body field.

Never log:

```text
Passwords
JWT tokens
Refresh tokens
Full connection strings
```

`SysUserCreated` / `SysUserModified` (from `BaseClass`) must be populated from the authenticated user, never from client input.

---

# 24. Logging

Logs should be useful and minimal.

Good:

```csharp
logger.LogWarning("Property {PropertyId} not found for landlord {LandlordId}", id, landlordId);
```

Avoid dumping entire request/response payloads or sensitive data. Never log credentials or tokens. Production logging should avoid unnecessary personal information.

---

# 25. Testing

Tests mirror the production folder structure exactly inside `SewaRent_Api.Tests/`.

```text
Features/Property/CreateProperty.cs        → Features/Property/CreatePropertyTests.cs
Controllers/Property/PropertyController.cs → Controllers/Property/PropertyControllerTests.cs
```

Test:

- Business logic and business-rule enforcement (e.g. landlord ownership, rental-request state transitions)
- Validation rules
- Data conversion / mapping to response DTOs
- Error handling
- Critical controller HTTP behavior (status codes, routing)

Not every feature needs a controller test — focus controller tests on HTTP concerns; feature/handler tests cover business logic. Use EF Core InMemory or a test DbContext fixture, never the production database.

Do not write tests simply to increase test count.

```csharp
// Features/Property/CreatePropertyTests.cs
public class CreatePropertyTests
{
    [Fact]
    public async Task Handle_ValidCommand_ReturnsMappedResponse()
    {
        // Arrange
        var db = TestDbFactory.CreatePropertyDb();
        var handler = new CreatePropertyHandler(db);
        var command = new CreatePropertyCommand(Guid.NewGuid(), "Nice Condo", null, 1500m, Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("Nice Condo", result.Title);
        Assert.Single(db.Properties);
    }
}
```

---

# 26. Test Naming

Use descriptive method names in `MethodUnderTest_Scenario_ExpectedResult` form.

Good:

```csharp
[Fact]
public async Task Handle_PropertyNotOwnedByLandlord_ThrowsForbidden() {}
```

```csharp
[Fact]
public async Task Handle_ValidRentalRequest_SetsStatusToPending() {}
```

Avoid:

```csharp
[Fact]
public async Task Test1() {}
```

---

# 27. Package (NuGet) Dependencies

Before adding a package:

1. Check whether .NET/BCL already provides the functionality.
2. Check whether an existing project dependency can solve it.
3. Add a package only when it provides meaningful value.
4. Avoid adding multiple packages that solve the same problem.
5. Document important architectural dependencies in `README.md`.

Do not add a package simply because it's popular. Do not introduce a second validation, mediator, or ORM library alongside FluentValidation/MediatR/EF Core.

---

# 28. AI Coding Agent Rules

AI agents working on the SewaRent API must follow these rules:

1. Read `README.md` before making architectural changes.
2. Read `INTEGRATION.md` before changing API integration/contracts.
3. Read `DATABASE.md` before making database-related assumptions.
4. Read this `CODING_STYLE.md` before generating any `.cs` file.
5. Follow the vertical-slice pattern: Command/Query + Validator + Response + Handler in one feature file.
6. Keep controllers thin — dispatch only, no business logic.
7. Entities must inherit `BaseClass` unless they are a composite-key junction table (see `DATABASE.md` §3.3).
8. Each data domain has its own `DbContext` and its own migrations subfolder — never share a `DbContext` across domains.
9. Never issue a physical `DELETE` against a `BaseClass`-derived table — always soft-delete via `IsDeleted`.
10. Never invent API endpoints not documented in `INTEGRATION.md`.
11. Never invent database columns or tables not documented in `DATABASE.md`.
12. Do not connect Flutter directly to MSSQL.
13. Do not create unnecessary abstractions (no `IPropertyFactory`/`IPropertyManager` when a handler + DbContext is enough).
14. Do not move files without a clear architectural reason.
15. Do not introduce a new NuGet dependency without justification.
16. Do not silently change existing behavior outside the requested task.
17. Keep comments minimal; do not add comments that repeat obvious code.
18. Run `dotnet format` after modifying `.cs` files.
19. Fix analyzer/nullable warnings introduced by the change.
20. Update documentation when architecture, endpoints, or database contracts change.
21. Preserve existing naming conventions.

---

# 29. Before Creating a New File

Before creating a new file, ask:

```text
Does this code have a clear responsibility?
Does an existing feature file already have the correct responsibility?
Does this belong to an existing domain, or does it need a new one?
Is this genuinely shared across domains?
Is the abstraction necessary?
```

Prefer:

```text
One clear feature file per operation
```

over:

```text
Many tiny files with no meaningful separation
```

---

# 30. Before Finishing a Change

Check:

```text
[ ] Code is formatted (dotnet format)
[ ] No unused usings
[ ] No unnecessary comments
[ ] No debug Console.WriteLine
[ ] No sensitive information in logs
[ ] No unnecessary abstractions
[ ] Business logic stays out of controllers and entities
[ ] Entity inherits BaseClass where applicable
[ ] Global query filter excludes IsDeleted rows
[ ] Naming follows project conventions
[ ] Validator added for every state-mutating Command
[ ] Tests added/updated where appropriate
[ ] Documentation updated if required
```

---

# 31. Comment Standard — Quick Reference

### Avoid

```csharp
// Create a new property
var property = new PropertyEntity();
```

### Prefer

```csharp
var property = new PropertyEntity();
```

### Use when needed

```csharp
// SewaRent business rule: a property cannot accept new requests while under offer.
if (property.AvailabilityStatus == PropertyAvailability.UnderOffer)
{
    throw new InvalidOperationException("Property is currently under offer.");
}
```

### Principle

> **Code explains what. Comments explain why.**

Keep comments short.

---

# 32. Final Coding Philosophy

The SewaRent API should prioritize:

```text
Readable
   ↓
Simple
   ↓
Consistent
   ↓
Testable
   ↓
Maintainable
```

Not:

```text
Complex
   ↓
Over-engineered
   ↓
Hard to understand
```

The best code is not the code with the most architecture. The best code is code where another developer — or an AI coding agent — can understand the intent quickly and safely make the next change.