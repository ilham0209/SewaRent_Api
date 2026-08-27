# SewaRent — Integration Specification

> This document defines the integration boundary between the **SewaRent Mobile App (Flutter)** and the future **SewaRent API (ASP.NET Core)**.
>
> It is intentionally written as an AI-agent reference so future agents can understand what the mobile app expects without guessing.

---

## 1. System Architecture

```text
┌──────────────────────────────┐
│      SewaRent Mobile         │
│       Flutter / Dart         │
└──────────────┬───────────────┘
               │
               │ HTTPS / REST / JSON
               ▼
┌──────────────────────────────┐
│        SewaRent API          │
│ ASP.NET Core Web API .NET 10 │
└──────────────┬───────────────┘
               │
               │ EF Core
               ▼
┌──────────────────────────────┐
│       Microsoft SQL Server   │
└──────────────────────────────┘
```

### Important rule

**Mobile must never connect directly to MSSQL.**

The only backend boundary exposed to Flutter is the HTTP API.

---

## 2. Integration Responsibilities

### Mobile responsibilities

- Render UI
- Collect user input
- Validate basic input
- Manage navigation
- Manage local UI state
- Store authentication token securely
- Send API requests
- Parse API responses
- Display API errors
- Handle loading/empty/error states

### API responsibilities

- Authentication
- Authorization
- Business rules
- Validation
- Database access
- Transaction management
- Entity relationships
- File/image handling
- Security
- Response formatting

### Database responsibilities

- Persist application data
- Enforce primary keys
- Enforce foreign keys
- Enforce required constraints
- Persist audit timestamps where appropriate

---

## 3. Base API Configuration

The mobile app should have one configurable base URL.

Concept:

```text
API_BASE_URL
```

Example environments:

```text
Development:
https://localhost:xxxx/api

Testing:
https://sewarent-api-test.example.com/api

Production:
https://sewarent-api.example.com/api
```

Actual URLs are to be configured when the API is created.

Do not hard-code environment-specific URLs inside feature files.

---

## 4. HTTP Standards

Expected methods:

| Method | Purpose |
|---|---|
| GET | Retrieve data |
| POST | Create data / actions |
| PUT | Replace/update data |
| PATCH | Partial update if required |
| DELETE | Delete/deactivate data |

Expected content type:

```text
application/json
```

For file upload:

```text
multipart/form-data
```

---

## 5. Authentication Integration

### Login

```text
POST /api/auth/login
```

Request concept:

```json
{
  "email": "user@example.com",
  "password": "password"
}
```

Response concept:

```json
{
  "accessToken": "...",
  "expiresAt": "...",
  "user": {
    "id": 1,
    "name": "User",
    "email": "user@example.com",
    "role": "Tenant"
  }
}
```

The exact API DTO names must be finalized when the API is implemented.

### Token usage

Authenticated requests use:

```text
Authorization: Bearer <accessToken>
```

### Logout

Logout should clear the locally stored token/session.

Server-side token revocation is optional depending on the final authentication design.

---

## 6. Authentication Endpoints

Planned:

```text
POST /api/auth/register
POST /api/auth/login
POST /api/auth/change-password
GET  /api/auth/me
```

Optional future endpoints:

```text
POST /api/auth/refresh
POST /api/auth/forgot-password
POST /api/auth/reset-password
```

---

## 7. Property Integration

> **Landlord scoping:** `GET /api/properties` and related endpoints are scoped to the authenticated tenant's linked landlord (`PR_Property.LandlordId == currentUser.LandlordId`) — this is no longer an open marketplace across all landlords. See §13 for how a tenant links to a landlord.

### Get properties

```text
GET /api/properties
```

Supported query parameters may include:

```text
keyword
location
minRent
maxRent
propertyTypeId
bedrooms
bathrooms
furnished
page
pageSize
sortBy
sortDirection
```

Example concept:

```text
GET /api/properties?location=Kuala%20Lumpur&minRent=800&maxRent=2000&page=1&pageSize=20
```

### Get property details

```text
GET /api/properties/{id}
```

### Create property

Landlord only:

```text
POST /api/properties
```

### Update property

Landlord/owner only:

```text
PUT /api/properties/{id}
```

### Deactivate property

```text
DELETE /api/properties/{id}
```

The final implementation may use a dedicated status endpoint instead of DELETE if soft-delete/deactivation is required.

---

## 8. PropertyType Integration

### Get property types

```text
GET /api/property-types
```

Returns all active property types (Apartment, Condo, Landed, Room, etc.).

### Create property type

Admin only:

```text
POST /api/property-types
```

Request body:

```json
{
  "name": "Studio",
  "description": "Self-contained single room unit"
}
```

---

## 9. Property Image Integration

### Upload image

Planned:

```text
POST /api/properties/{propertyId}/images
```

Content:

```text
multipart/form-data
```

### Delete image

```text
DELETE /api/properties/{propertyId}/images/{imageId}
```

The API should return a public/authorized image URL or image identifier that the mobile application can render.

---

## 10. Favourite Integration

### Get current user's favourites

```text
GET /api/favourites
```

### Add favourite

```text
POST /api/favourites
```

Request:

```json
{
  "propertyId": 1
}
```

### Remove favourite

```text
DELETE /api/favourites/{propertyId}
```

The API should use the authenticated user from the JWT rather than accepting an arbitrary `userId` from the mobile app.

---

## 11. Rental Request Integration

### Create rental request

```text
POST /api/rental-requests
```

Concept:

```json
{
  "propertyId": 1,
  "message": "I am interested in renting this property."
}
```

### Get tenant requests

```text
GET /api/rental-requests/my
```

### Get request details

```text
GET /api/rental-requests/{id}
```

### Cancel request

```text
POST /api/rental-requests/{id}/cancel
```

### Landlord requests

```text
GET /api/landlord/rental-requests
```

### Approve request

```text
POST /api/rental-requests/{id}/approve
```

### Reject request

```text
POST /api/rental-requests/{id}/reject
```

The API must verify that the authenticated landlord owns the property associated with the request.

---

## 12. Profile Integration

### Get profile

```text
GET /api/users/me
```

### Update profile

```text
PUT /api/users/me
```

Concept:

```json
{
  "fullName": "User Name",
  "phoneNumber": "0123456789"
}
```

### Profile image

Planned:

```text
POST /api/users/me/profile-image
```

---

## 13. Landlord Linking & Bank Details Integration

### Get landlord code (landlord's own profile)

Already part of profile response (§11):

```text
GET /api/users/me
```

Response concept adds:

```json
{
  "landlordCode": "LL-260827-01",
  "bankName": "Maybank",
  "bankAccountNumber": "1234567890"
}
```

### Update bank details (landlord only)

```text
PUT /api/users/me/bank-details
```

Request concept:

```json
{
  "bankName": "Maybank",
  "bankAccountNumber": "1234567890"
}
```

Editable at any time. Does not retroactively change bank details already snapshotted onto issued invoices — see `DATABASE.md` §14.

### Link tenant to landlord

```text
POST /api/auth/link-landlord
```

Request concept:

```json
{
  "landlordCode": "LL-260827-01"
}
```

Response concept:

```json
{
  "success": true,
  "message": null,
  "data": {
    "landlordId": "..."
  }
}
```

The API resolves `landlordCode` to a `landlordId` server-side and stores it on the authenticated tenant's own record. The mobile app must never send a raw `landlordId`.

### Property visibility rule

Once linked, all property-browsing endpoints (`GET /api/properties`, search, filters) return only properties where:

```text
PR_Property.LandlordId == currentUser.LandlordId
```

A tenant with no linked landlord receives an empty list, not an error, and the mobile app should show a "link to your landlord" empty state prompting for the landlord code.

---

## 14. Billing & Invoice Integration

### Get invoice details

```text
GET /api/invoices/{id}
```

Response concept:

```json
{
  "invoiceNumber": "INV-2026-08-0001",
  "billingPeriodMonth": 8,
  "billingPeriodYear": 2026,
  "items": [
    { "itemType": "Rent", "description": "Monthly rent", "amount": 1200.00 },
    { "itemType": "Water", "description": "Water bill", "amount": 25.00 }
  ],
  "totalAmount": 1225.00,
  "status": "Unpaid",
  "dueDate": "...",
  "bankName": "Maybank",
  "bankAccountNumber": "1234567890"
}
```

`bankName` / `bankAccountNumber` on the invoice are the **snapshot** values, not the landlord's live profile.

### List invoices

```text
GET /api/invoices/my              (tenant — own invoices)
GET /api/landlord/invoices        (landlord — invoices across their tenants)
```

Supports the same style of query params as property listing (`status`, `page`, `pageSize`).

### Payment already made (tenant)

```text
POST /api/invoices/{id}/mark-paid-claim
```

No file/attachment required. Sets `Status = PaymentClaimed` and notifies the landlord.

### Accept payment (landlord)

```text
POST /api/invoices/{id}/accept-payment
```

Sets `Status = Paid`, `PaidDate = now()`, and auto-generates a receipt. The API must verify the authenticated landlord owns the property behind the invoice.

### Reject payment (landlord)

```text
POST /api/invoices/{id}/reject-payment
```

Request concept:

```json
{
  "reason": "Amount transferred does not match the invoice total."
}
```

`reason` is **required** — the API returns `422` if missing. Sets `Status = Unpaid`; the same invoice/due date is reused (no new invoice is generated).

### Download PDF

```text
GET /api/invoices/{id}/pdf
GET /api/receipts/{id}/pdf
```

Returns a PDF file (`application/pdf`). Payment gateway integration is out of scope for now — invoices/receipts document a manual bank-transfer workflow.

---

## 15. Payment Notification & Dashboard Integration

### Landlord: configure scheduled reminder

```text
PUT /api/rental-requests/{id}/payment-schedule
```

Request concept:

```json
{
  "scheduleDay": 1
}
```

### Landlord: send manual reminder

```text
POST /api/rental-requests/{id}/payment-reminder
```

Every call creates one notification record; it does not generate an invoice.

### Get notifications

```text
GET /api/notifications/my
```

Returns notifications scoped to the authenticated user's role — a tenant sees `Scheduled`/`Manual` reminders, a landlord sees `Overdue` notices and payment-claimed alerts.

### Dashboard

```text
GET /api/dashboard/landlord
GET /api/dashboard/tenant
```

Landlord dashboard response concept:

```json
{
  "totalCollectedThisMonth": 3600.00,
  "overdueCount": 2,
  "tenants": [
    { "tenantName": "...", "propertyTitle": "...", "invoiceStatus": "Unpaid", "dueDate": "..." }
  ]
}
```

Tenant dashboard response concept:

```json
{
  "currentInvoice": { "status": "Unpaid", "totalAmount": 1225.00, "dueDate": "..." },
  "history": [
    { "invoiceNumber": "...", "status": "Paid", "hasReceipt": true }
  ]
}
```

Both dashboards link out to `GET /api/invoices/{id}/pdf` and `GET /api/receipts/{id}/pdf` for each history row.

---

## 16. API Response Standard

The final API should use a consistent response format.

Recommended success shape:

```json
{
  "success": true,
  "message": null,
  "data": {}
}
```

Recommended error shape:

```json
{
  "success": false,
  "message": "The property could not be found.",
  "errors": []
}
```

For validation:

```json
{
  "success": false,
  "message": "Validation failed.",
  "errors": [
    {
      "field": "email",
      "message": "Email is required."
    }
  ]
}
```

The exact response envelope can be changed before API implementation, but once finalized it must be treated as a contract.

---

## 17. HTTP Status Code Contract

| Status | Meaning | Mobile behavior |
|---|---|---|
| 200 | Success | Process response |
| 201 | Created | Process created resource |
| 204 | Success, no content | Complete action |
| 400 | Bad request | Show request/validation error |
| 401 | Unauthenticated | Clear/refresh session and redirect to login |
| 403 | Forbidden | Show permission message |
| 404 | Not found | Show not-found state |
| 409 | Conflict | Show conflict/business-rule message |
| 422 | Validation failure | Show field errors |
| 500 | Server error | Show generic retry message |

---

## 18. Mobile API Client

Recommended responsibility:

```text
core/network/api_client.dart
```

The API client should handle:

- Base URL
- HTTP method
- Headers
- Authorization header
- JSON encoding
- JSON decoding
- Timeout
- HTTP errors

Feature code should not manually construct raw HTTP requests everywhere.

---

## 19. Repository Boundary

Recommended flow:

```text
Page
  ↓
Controller / State
  ↓
Use Case (if required)
  ↓
Repository
  ↓
Remote Data Source
  ↓
ApiClient
  ↓
HTTP API
```

This prevents UI code from becoming coupled to HTTP implementation details.

---

## 20. Authentication Storage

JWT access tokens must be stored using secure storage.

Do not use ordinary preferences for sensitive tokens unless the final security design explicitly accepts the risk.

Concept:

```text
Login
  ↓
API returns token
  ↓
Secure Storage
  ↓
ApiClient reads token
  ↓
Authorization: Bearer <token>
```

Never log the token.

---

## 21. Pagination

Property listing should support pagination from the beginning.

Recommended request:

```text
?page=1&pageSize=20
```

Recommended response concept:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalCount": 100,
  "totalPages": 5
}
```

The exact pagination contract will be finalized in the API.

---

## 22. Image Handling

The mobile application should:

- Show loading placeholders
- Handle failed image loading
- Cache images where appropriate
- Avoid downloading unnecessarily large images
- Use API-provided URLs
- Never assume a local server path is publicly accessible

The API should provide mobile-consumable image URLs.

---

## 23. Offline / Network Handling

At minimum, the mobile application should distinguish:

```text
Loading
Success
Empty
Network Error
Unauthorized
Server Error
```

Future versions may add offline caching.

---

## 24. Integration Matrix

| Mobile Feature | API | Database Area |
|---|---|---|
| Register | `/auth/register` | Users |
| Login | `/auth/login` | Users |
| Profile | `/users/me` | Users |
| Browse properties | `/properties` | Properties |
| Property details | `/properties/{id}` | Properties + Images + Types |
| Search/filter | `/properties` | Properties + related tables |
| Add favourite | `/favourites` | Favourites |
| Remove favourite | `/favourites/{propertyId}` | Favourites |
| Tenant requests | `/rental-requests/my` | RentalRequests |
| Create request | `/rental-requests` | RentalRequests |
| Landlord requests | `/landlord/rental-requests` | RentalRequests |
| Approve/reject | `/rental-requests/{id}/...` | RentalRequests + Properties |
| Add property | `/properties` | Properties |
| Edit property | `/properties/{id}` | Properties |
| Property image | `/properties/{id}/images` | PropertyImages |
| Link to landlord | `/auth/link-landlord` | Users |
| Update bank details | `/users/me/bank-details` | Users |
| Invoice details | `/invoices/{id}` | Invoices + InvoiceItems |
| Tenant invoice list | `/invoices/my` | Invoices |
| Landlord invoice list | `/landlord/invoices` | Invoices |
| Payment already made | `/invoices/{id}/mark-paid-claim` | Invoices |
| Accept payment | `/invoices/{id}/accept-payment` | Invoices + Receipts |
| Reject payment | `/invoices/{id}/reject-payment` | Invoices |
| Invoice/receipt PDF | `/invoices/{id}/pdf`, `/receipts/{id}/pdf` | Invoices + Receipts |
| Scheduled reminder config | `/rental-requests/{id}/payment-schedule` | PaymentNotifications |
| Manual reminder | `/rental-requests/{id}/payment-reminder` | PaymentNotifications |
| Notifications | `/notifications/my` | PaymentNotifications |
| Landlord dashboard | `/dashboard/landlord` | Invoices + RentalRequests |
| Tenant dashboard | `/dashboard/tenant` | Invoices + Receipts |

---

## 25. AI Agent Rules

Before changing an API integration:

1. Read `README.md`.
2. Read this file.
3. Check the endpoint exists in this document.
4. Do not invent an endpoint.
5. Check the request/response contract.
6. Check authentication requirements.
7. Check the relevant database mapping in `DATABASE.md`.
8. Update this file if the endpoint changes.
9. Do not bypass the repository/API-client boundary.
10. Do not connect Flutter directly to MSSQL.

---

## 26. Endpoint Status

The endpoints in this document are **planned contracts**, not currently implemented endpoints.

When the backend is created, each endpoint should be marked:

```text
[PLANNED]
[IMPLEMENTED]
[TESTED]
[DEPRECATED]
```

Example:

```text
POST /api/auth/login
Status: PLANNED
```

This prevents an AI agent from assuming that a documented endpoint already exists.

---

## 27. Integration Checklist

### Authentication

- [ ] Register
- [ ] Login
- [ ] Token storage
- [ ] Authorization header
- [ ] Token expiry
- [ ] Logout
- [ ] Change password

### Properties

- [ ] List
- [ ] Search
- [ ] Filter
- [ ] Details
- [ ] Create
- [ ] Update
- [ ] Deactivate
- [ ] Images

### Favourites

- [ ] List
- [ ] Add
- [ ] Remove

### Rental Requests

- [ ] Create
- [ ] List tenant requests
- [ ] Details
- [ ] Cancel
- [ ] Landlord list
- [ ] Approve
- [ ] Reject

### Profile

- [ ] Get
- [ ] Update
- [ ] Profile image
- [ ] Bank details (landlord)

### Landlord Linking

- [ ] Landlord code shown on landlord profile
- [ ] Tenant link via landlord code
- [ ] Property visibility scoped to linked landlord

### Billing

- [ ] Invoice details
- [ ] Tenant invoice list
- [ ] Landlord invoice list
- [ ] Payment already made (tenant)
- [ ] Accept payment (landlord)
- [ ] Reject payment with reason (landlord)
- [ ] Invoice PDF download
- [ ] Receipt PDF download

### Payment Notification

- [ ] Scheduled reminder config
- [ ] Manual reminder
- [ ] Overdue notice (landlord)
- [ ] Notification list

### Dashboard

- [ ] Landlord dashboard summary
- [ ] Tenant dashboard summary