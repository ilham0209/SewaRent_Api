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

```text
SewaRent_Api/
│
├── SewaRent_Api/
│   │
│   ├── Controllers/
│   │   └── Example/
│   │       └── ExampleController.cs
│   │
│   ├── Features/
│   │   └── Example/
│   │       ├── CreateExample.cs
│   │       ├── DeleteExample.cs
│   │       ├── GetAllExample.cs
│   │       ├── GetByIdExample.cs
│   │       └── UpdateExample.cs
│   │
│   ├── Properties/
│   │   └── launchSettings.json
│   │
│   ├── Shared/
│   │   ├── Domain/
│   │   │   ├── BaseClass.cs
│   │   │   ├── Car/
│   │   │   │   └── CarEntities.cs
│   │   │   └── Example/
│   │   │       └── ExampleEntities.cs
│   │   ├── Extensions/
│   │   │   └── QueryableExtensions.cs
│   │   ├── Infrastructure/
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
ExampleController.cs
CreateExample.cs
ExampleEntities.cs
```

### Classes

Use `PascalCase`:

```csharp
public class ExampleController {}
public class CreateExample {}
```

### Methods and variables

Use `PascalCase` for methods, `camelCase` for local variables:

```csharp
public async Task<IActionResult> GetAllAsync() {}
var example = new ExampleEntities();
```

### Folders

Folders follow the module/group naming pattern:

```text
{PROJECT NAME}.API.{GROUP NAME}
```

Example: `SewaRent_Api.FINANCIAL`

For test folders:

```text
{PROJECT NAME}.API.{GROUP NAME}.Tests
```

Example: `SewaRent_Api.FINANCIAL.Tests`

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
Microsoft SQL Server
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
MSSQL
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

### Phase 1 — API Foundation (current example baseline)

- [x] Create solution and project structure
- [x] Configure EF Core and DbContext
- [x] Configure migrations
- [x] Configure CORS
- [x] Configure OpenAPI + Scalar
- [x] Add global exception handling middleware
- [ ] Add DataGrid list infrastructure

### Phase 2 — Example Domain (current)

- [x] Example module (controller, features, entities)
- [x] Car module (entities, DbContext, migrations)

### Phase 3 — SewaRent Backend

- [ ] Create database schema for SewaRent
- [ ] Implement authentication (JWT)
- [ ] Implement property APIs
- [ ] Implement favourite APIs
- [ ] Implement rental request APIs
- [ ] Implement profile APIs
- [ ] Implement image upload

### Phase 4 — Integration Support

- [ ] Publish API contract for mobile (INTEGRATION.md)
- [ ] Document database schema (DATABASE.md)
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

Connection strings and settings are stored in `appsettings.json` and environment-specific files. Sensitive values must not be committed to source control.

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
| `INTEGRATION.md` | Mobile ↔ API integration contract (planned) |
| `DATABASE.md` | Database tables and feature-to-table mapping (planned) |

These documents should be kept updated as the system evolves.

---

## 16. AI Agent Development Rules

AI coding agents working on SewaRent should:

1. Read `README.md` before changing architecture.
2. Read `INTEGRATION.md` before changing API-related code.
3. Read `DATABASE.md` before proposing database-related changes.
4. Never make Flutter connect directly to MSSQL.
5. Never invent API endpoints if they are not documented.
6. Never invent database columns when implementing API integration.
7. Keep feature-specific code inside the relevant feature folder.
8. Avoid moving files unless there is a clear architectural reason.
9. Preserve existing naming conventions.
10. Update documentation when adding or changing features, endpoints, or database relationships.
11. Do not introduce a new package when existing project functionality is sufficient.
12. Do not perform broad refactors unrelated to the requested task.

---

## 17. Current Status

**Project:** SewaRent  
**Platform:** API backend  
**Framework:** ASP.NET Core Web API  
**Language:** C#  
**Version:** .NET 10  
**Database:** Microsoft SQL Server  
**Current Stage:** Phase 1/2 — Foundation + Example domain baseline

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
    └── DATABASE.md
```

The API backend remains independently deployable from the mobile application.

---

## Quick start

1. Clone this project to a local machine.
2. Open the solution (`SewaRent_Api.sln`) in Visual Studio.
3. Edit `appsettings.json` accordingly for the database connection.
4. Run the solution; Visual Studio will open a browser.
5. Manually navigate to `https://localhost:7062/scalar` to view the API reference.

## Steps to start developing modules and features

1. Decide a domain name and create a folder in `Shared/Domain/`.
2. Create the entity in `Shared/Domain/{DOMAIN_NAME}/`, e.g. `Shared/Domain/Example/ExampleEntities.cs`.
3. Create a migration file for the entity:
   1. In Package Manager Console run `Add-Migration -Name {NAME} -Context SewaRentDbContext`.
   2. Migration files are created in `Shared/Infrastructure/Migrations/` with a timestamp.
4. Run the solution; migrations run automatically to the latest version.
5. Implement the feature.
