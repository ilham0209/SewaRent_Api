# SewaRent — API Backend

> **SewaRent** is a rental-property application. This repository contains the **API backend (API Ver)**.
>
> This document is the main project reference for the **SewaRent API**. It describes the product scope, features, architecture, file structure, conventions, integration boundaries, and development roadmap.

---

## 1. Project Overview

The SewaRent API is the backend for the SewaRent mobile application. It exposes REST endpoints over HTTPS that the Flutter app consumes.

The API is responsible for:

- Authentication and authorization
- Business rules
- Data validation
- Database access (via Entity Framework Core)

### Main principle

The Flutter mobile application **never connects directly to MSSQL**.

```text
Flutter Mobile
      |
      | HTTPS / REST / JSON
      v
SewaRent API            <-- this repository
      |
      | EF Core
      v
Microsoft SQL Server
```

The mobile application communicates **only** with the SewaRent API.

---

## 2. Technology Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core Web API |
| Backend Version | .NET 10 |
| Language | C# |
| ORM | Entity Framework Core |
| Database | Microsoft SQL Server |
| Database Tool | SQL Server Management Studio (SSMS) |
| Authentication | JWT Bearer (planned) |
| API Format | REST + JSON |
| API Transport | HTTPS |
| API Documentation | OpenAPI + Scalar |
| CQRS / Request Pipeline | MediatR |
| Validation | FluentValidation |
| IDE | Visual Studio |
| Source Control | Git / GitHub |

---

## 3. User Roles

### Tenant

A tenant can:

- Register
- Login
- Browse, search, and filter properties
- View property details and images
- Save/unsave favourites
- Submit rental requests
- View rental request status
- Cancel eligible rental requests
- Manage profile

### Landlord

A landlord can:

- Register
- Login
- Manage their properties (add, edit, deactivate, upload images)
- View incoming rental requests
- Accept/reject rental requests
- Manage profile

### Administrator

Administrator functionality is planned for the backend/admin side.

Possible responsibilities:

- Manage users
- Manage properties
- Manage property categories/types
- Review reported content
- Manage rental requests
- View system statistics
- Manage system configuration

---

## 4. API Modules

The API is organised by feature. Each module follows the pattern `Controller -> Features -> Shared`.

### 4.1 Authentication

Planned endpoints:

- Login
- Register
- Refresh token
- Logout

### 4.2 Property

Planned endpoints:

- List properties (keyword, location, price range, type, bedrooms, bathrooms, furnished, availability)
- Sort by rent, newest, relevance
- Get property details
- Get property images
- Landlord CRUD: create, update, deactivate property
- Upload/remove property images

### 4.3 Favourite

Planned endpoints:

- Add property to favourites
- Remove property from favourites
- List favourite properties

### 4.4 Rental Request

Planned endpoints:

- Submit rental request
- List own rental requests (tenant)
- Get rental request details
- Cancel eligible rental request
- List incoming rental requests (landlord)
- Accept/reject rental request (landlord)

Possible statuses:

```text
Pending
Approved
Rejected
Cancelled
Expired
```

### 4.5 Profile

Planned endpoints:

- Get profile
- Update profile
- Update phone number
- Update profile image
- Change password

---

## 5. API Mapping (Mobile → API)

The mobile app maps each screen to API endpoints:

| Mobile screen | API module |
|---|---|
| Splash / Login / Register | Authentication |
| Home / Property Search | Property |
| Property Details | Property |
| Favourites | Favourite |
| Rental Requests | Rental Request |
| Profile | Profile |

---

## 6. API File Structure

This is the current target structure for the SewaRent domains, following the `imas-dotnet-architecture` vertical-slice convention. `Controllers/` and `Features/` are organised **per business capability**; `Shared/Domain/` is organised **per data domain** with one file per entity table — so `Auth` and `Profile` share the same `User` domain (both operate on `Users`/`Roles`/`UserRoles`), and `Property` bundles `PropertyTypes` and `PropertyImages` together with `Properties`. All domains share a **single database** named `SewaRent` via a single `SewaRentDbContext`.

```text
SewaRent_Api/
│
├── SewaRent_Api/
│   │
│   ├── Controllers/
│   │   ├── Auth/
│   │   │   └── AuthController.cs
│   │   ├── Profile/
│   │   │   └── ProfileController.cs
│   │   ├── Property/
│   │   │   ├── PropertyController.cs
│   │   │   ├── PropertyTypeController.cs
│   │   │   └── PropertyImageController.cs
│   │   ├── Favourite/
│   │   │   └── FavouriteController.cs
│   │   └── RentalRequest/
│   │       ├── RentalRequestController.cs
│   │       └── LandlordRentalRequestController.cs
│   │
│   ├── Features/
│   │   ├── Auth/
│   │   │   ├── Register.cs
│   │   │   ├── Login.cs
│   │   │   └── ChangePassword.cs
│   │   ├── Profile/
│   │   │   ├── GetProfile.cs
│   │   │   ├── UpdateProfile.cs
│   │   │   └── UpdateProfileImage.cs
│   │   ├── Property/
│   │   │   ├── CreateProperty.cs
│   │   │   ├── UpdateProperty.cs
│   │   │   ├── DeactivateProperty.cs
│   │   │   ├── DeleteProperty.cs
│   │   │   ├── GetAllProperty.cs
│   │   │   ├── GetByIdProperty.cs
│   │   │   ├── UploadPropertyImage.cs
│   │   │   ├── DeletePropertyImage.cs
│   │   │   ├── GetAllPropertyType.cs
│   │   │   └── CreatePropertyType.cs
│   │   ├── Favourite/
│   │   │   ├── AddFavourite.cs
│   │   │   ├── RemoveFavourite.cs
│   │   │   └── GetAllFavourite.cs
│   │   └── RentalRequest/
│   │       ├── CreateRentalRequest.cs
│   │       ├── GetMyRentalRequest.cs
│   │       ├── GetByIdRentalRequest.cs
│   │       ├── GetLandlordRentalRequest.cs
│   │       ├── CancelRentalRequest.cs
│   │       ├── ApproveRentalRequest.cs
│   │       └── RejectRentalRequest.cs
│   │
│   ├── Properties/
│   │   └── launchSettings.json
│   │
│   ├── Shared/
│   │   ├── Domain/
│   │   │   ├── BaseClass.cs
│   │   │   ├── User/
│   │   │   │   ├── US_Users.cs
│   │   │   │   ├── US_Roles.cs
│   │   │   │   └── US_UserRoles.cs
│   │   │   ├── Property/
│   │   │   │   ├── PR_Property.cs
│   │   │   │   ├── PR_PropertyTypes.cs
│   │   │   │   └── PR_PropertyImages.cs
│   │   │   ├── Favourite/
│   │   │   │   └── FA_Favourites.cs
│   │   │   └── RentalRequest/
│   │   │       ├── RR_RentalRequests.cs
│   │   │       └── RR_RentalRequestStatuses.cs
│   │   ├── Extensions/
│   │   │   └── QueryableExtensions.cs
│   │   ├── Infrastructure/
│   │   │   ├── Behavior/
│   │   │   │   └── ValidationBehavior.cs
│   │   │   ├── Migrations/
│   │   │   └── Persistence/
│   │   │       └── SewaRentDbContext.cs
│   │   ├── Middleware/
│   │   │   └── GlobalExceptionHandlingMiddleware.cs
│   │   └── Models/
│   │       ├── DataGridRequest.cs
│   │       └── DataGridResponse.cs
│   │
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Program.cs
```

Domain-to-table reference (see `DATABASE.md` for full column definitions):

| Domain | Entities | Tables |
|---|---|---|
| `User` | `UserEntity`, `RoleEntity`, `UserRoleEntity` | `US_Users`, `US_Roles`, `US_UserRoles` |
| `Property` | `PropertyEntity`, `PropertyTypeEntity`, `PropertyImageEntity` | `PR_Property`, `PR_PropertyTypes`, `PR_PropertyImages` |
| `Favourite` | `FavouriteEntity` | `FA_Favourites` |
| `RentalRequest` | `RentalRequestEntity`, `RentalRequestStatusEntity` | `RR_RentalRequests`, `RR_RentalRequestStatuses` |

All domains share a **single database** named `SewaRent`. Each domain uses a table prefix to avoid naming collisions: `US_` (User), `PR_` (Property), `FA_` (Favourite), `RR_` (RentalRequest).

All project code resides in `SewaRent_Api`.

### Feature notes

- `AuthController` and `ProfileController` are separate controllers/feature folders (different HTTP concerns: unauthenticated auth flow vs. authenticated self-service profile), but both read/write through `SewaRentDbContext` via the `User` domain.
- `PropertyController` handles property CRUD/search; `PropertyImageController` is split out because image upload uses `multipart/form-data` and a distinct request/response shape, even though it shares `SewaRentDbContext`.
- `RentalRequestController` is the tenant-facing surface (`create`, `my`, `{id}`, `cancel`); `LandlordRentalRequestController` is the landlord-facing surface (`list`, `approve`, `reject`). Both use `SewaRentDbContext` — the split mirrors the two different authorization rules (tenant owns the request vs. landlord owns the property).
- New modules from the roadmap (Payments, Notifications, Reports, etc.) should each get their own `Controllers/{Domain}`, `Features/{Domain}`, `Shared/Domain/{Domain}`, and corresponding table prefix when their requirements are approved — do not fold them into an existing domain unless they genuinely share the same tables.

---

## 7. Folder Responsibilities

### `Program.cs`

Application entry point. Registers services, middleware, EF Core context, and applies migrations on startup.

### `Controllers/`

Thin HTTP layer. Controllers receive requests, delegate to feature handlers, and return responses.

### `Features/`

Business features, one file per operation (CQRS/MediatR style).

### `Properties/`

Visual Studio launch settings.

### `Shared/`

Cross-feature technical functionality shared across modules:

- `Domain/` — entities and base classes (one file per entity table)
- `Extensions/` — reusable query extensions
- `Infrastructure/` — EF Core DbContext and migrations
- `Middleware/` — global exception handling
- `Models/` — reusable request/response models (e.g. `DataGridRequest`, `DataGridResponse`)

---

## 8. Naming Conventions

Use standard .NET/C# conventions.

### Files

Use `PascalCase`:

```text
PropertyController.cs
CreateProperty.cs
PropertyEntities.cs
```

### Classes

Use `PascalCase`:

```csharp
public class PropertyController {}
public class CreateProperty {}
```

### Methods and variables

Use `PascalCase` for methods, `camelCase` for local variables:

```csharp
public async Task<IActionResult> GetAllAsync() {}
var property = new PropertyEntity();
```

### Folders

Folders follow the module/group naming pattern:

```text
{PROJECT NAME}.API.{GROUP NAME}
```

Example: `SewaRent_Api.PROPERTY`

For test folders:

```text
{PROJECT NAME}.API.{GROUP NAME}.Tests
```

Example: `SewaRent_Api.PROPERTY.Tests`

---

## 9. Architecture Conventions

### Request pipeline

Controllers are thin. Business logic lives in `Features/` as MediatR requests and handlers:

```text
Controller
      |
      v
MediatR Request Handler (Features)
      |
      v
EF Core DbContext (Shared/Infrastructure/Persistence)
      |
      v
Microsoft SQL Server (SewaRent database)
```

### Validation

Requests are validated with FluentValidation before they reach business logic.

### Pagination / grid requests

List endpoints use `DataGridRequest` / `DataGridResponse` and `QueryableExtensions` for filtering, sorting, and paging.

---

## 10. API Integration Principle

The mobile application communicates only with the SewaRent API. The API is the single entry point for the mobile app.

```text
PropertyPage (Flutter)
      |
      v
PropertyController
      |
      v
PropertyRepository
      |
      v
ApiClient
      |
      | HTTPS / REST / JSON
      v
SewaRent API      <-- this repository
      |
      | EF Core
      v
MSSQL (SewaRent database)
```

The API then communicates with MSSQL. Endpoints must be documented before the mobile app implements them.

---

## 11. Error Handling

The API should return meaningful HTTP status codes and friendly error messages without exposing internal server or database errors.

Status codes to support:

```text
400  Bad Request
401  Unauthorized
403  Forbidden
404  Not Found
409  Conflict
422  Unprocessable Entity
500  Internal Server Error
```

`GlobalExceptionHandlingMiddleware` converts unhandled exceptions into a consistent error response.

---

## 12. Security Rules

Never:

- Store MSSQL credentials in Flutter
- Allow the Flutter app to connect directly to MSSQL
- Hard-code JWT secrets
- Store sensitive credentials in source control
- Log passwords
- Log JWT tokens
- Trust role information from the UI alone

The API must enforce authorization. The backend is the security boundary; the mobile app only controls what UI is shown.

---

## 13. Development Roadmap

### Phase 1 — API Foundation (baseline)

- [x] Create solution and project structure
- [x] Configure EF Core and DbContext
- [x] Configure migrations
- [x] Configure CORS
- [x] Configure OpenAPI + Scalar
- [x] Add global exception handling middleware
- [ ] Add DataGrid list infrastructure

### Phase 2 — SewaRent Domains

- [ ] User domain (entities, migrations) — backs Auth + Profile
- [ ] Property domain (entities, migrations) — Properties, PropertyTypes, PropertyImages
- [ ] Favourite domain (entities, migrations)
- [ ] RentalRequest domain (entities, migrations) — RentalRequests, RentalRequestStatuses

### Phase 3 — SewaRent Backend

- [ ] Create database schema for SewaRent
- [ ] Implement authentication (JWT)
- [ ] Implement property APIs
- [ ] Implement favourite APIs
- [ ] Implement rental request APIs
- [ ] Implement profile APIs
- [ ] Implement image upload

### Phase 4 — Integration Support

- [ ] Publish API contract for mobile (`INTEGRATION.md`)
- [ ] Document database schema (`DATABASE.md`)
- [ ] Environment-specific configuration
- [ ] Production deployment

### Phase 5 — Advanced Features

- [ ] Notifications
- [ ] Landlord dashboard APIs
- [ ] Reporting / analytics
- [ ] Admin module

---

## 14. Environment Configuration

Different environments should be supported:

```text
Development
Testing
Production
```

Connection strings and settings are stored in `appsettings.json` and environment-specific files. Sensitive values must not be committed to source control. All environments connect to a single database named `SewaRent`.

Example concept:

```text
Development:
https://localhost:7062

Testing:
https://sewarent-api-test.example.com

Production:
https://sewarent-api.example.com
```

Actual URLs will be configured later.

---

## 15. Documentation

The project documentation is split into:

| Document | Purpose |
|---|---|
| `README.md` | API project overview and architecture |
| `INTEGRATION.md` | Mobile ↔ API integration contract |
| `DATABASE.md` | Database tables and feature-to-table mapping |
| `CODING_STYLE.md` | C#/.NET coding style and clean-code rules |

---

## 16. AI Agent Development Rules

AI coding agents working on SewaRent should:

1. Read `README.md` before changing architecture.
2. Read `INTEGRATION.md` before changing API-related code.
3. Read `DATABASE.md` before proposing database-related changes.
4. Read `CODING_STYLE.md` before generating any `.cs` file.
5. Never make Flutter connect directly to MSSQL.
6. Never invent API endpoints if they are not documented.
7. Never invent database columns when implementing API integration.
8. Keep feature-specific code inside the relevant feature folder.
9. Avoid moving files unless there is a clear architectural reason.
10. Preserve existing naming conventions.
11. Update documentation when adding or changing features, endpoints, or database relationships.
12. Do not introduce a new package when existing project functionality is sufficient.
13. Do not perform broad refactors unrelated to the requested task.

---

## 17. Current Status

**Project:** SewaRent
**Platform:** API backend
**Framework:** ASP.NET Core Web API
**Language:** C#
**Version:** .NET 10
**Database:** Microsoft SQL Server — single database `SewaRent`
**Current Stage:** Phase 1/2 — Foundation + SewaRent domain scaffolding

---

## 18. Future Repository Structure

The complete solution is expected to become:

```text
SewaRent/
│
├── SewaRent_Api/
│   └── ASP.NET Core Web API (this repository)
│
├── SewaRent.Mobile/
│   └── Flutter application
│
├── SewaRent.Database/
│   └── SQL/database scripts (if required)
│
└── docs/
    ├── README.md
    ├── INTEGRATION.md
    ├── DATABASE.md
    └── CODING_STYLE.md
```

The API backend remains independently deployable from the mobile application.

---

## Quick start

1. Clone this project to a local machine.
2. Open the solution (`SewaRent_Api.sln`) in Visual Studio.
3. Edit `appsettings.json` accordingly for the database connection (single `SewaRent` database).
4. Run the solution; Visual Studio will open a browser.
5. Manually navigate to `https://localhost:7062/scalar` to view the API reference.

## Steps to start developing modules and features

1. Decide a domain name and create a folder in `Shared/Domain/`.
2. Create the entity file(s) in `Shared/Domain/{DOMAIN_NAME}/`, named after the table, e.g. `Shared/Domain/Property/PR_Property.cs` (one file per table).
3. Create a migration file for the entity:
    1. In Package Manager Console run `Add-Migration -Name {NAME} -Context SewaRentDbContext`.
   2. Migration files are created in `Shared/Infrastructure/Migrations/` with a timestamp.
4. Run the solution; migrations run automatically to the latest version.
5. Implement the feature.
