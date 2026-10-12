---
name: api-design
description: >
  Designs and reviews production-quality JSON REST APIs for .NET following
  Stripe/GitHub-level conventions. Uses PUT semantics instead of PATCH to
  avoid JSON serializer null/default ambiguity. Use when the user asks to
  "design an API", "review my API", "REST API design", "API endpoints",
  "add pagination", "API error handling", "resource design", or "design
  the routes". Do NOT use for general code review, architecture review,
  GraphQL, gRPC, or message queue design.
allowed-tools: Read Grep Glob Agent
metadata:
  author: Ryan Buckley
  version: "1.0"
---

# JSON REST API Design

Design APIs that are consistent, predictable, and a joy to integrate with.

## Important

Non-negotiable constraints for .NET JSON APIs:

1. **No PATCH, no partial updates via POST.** Plain `System.Text.Json` POCO binding collapses a property set to `null`/default and a property not sent (distinguishing them takes `required` members or custom converters). Use PUT with full resource replacement.
2. **Action endpoints for state transitions.** When an update isn't "replace the whole resource," model it as a verb: `POST /v1/orders/{id}/cancel`.
3. **Every design decision should reduce ambiguity for the API consumer.**

## Core Principles

### Resource-Oriented Design

URLs identify resources (nouns), not operations (verbs). Resources are plural.

```
GET    /v1/customers              # List
POST   /v1/customers              # Create
GET    /v1/customers/{id}         # Retrieve
PUT    /v1/customers/{id}         # Full replace
DELETE /v1/customers/{id}         # Delete
```

Nest resources only when there is strong ownership and the child is never accessed independently:

```
GET    /v1/customers/{id}/addresses
POST   /v1/customers/{id}/addresses
PUT    /v1/customers/{id}/addresses/{addressId}
```

If a resource is frequently accessed on its own, promote it to top-level with a filter:

```
GET    /v1/invoices?customerId={id}
```

### The PUT-First Update Model

The update flow is: **GET, modify, PUT**.

```http
GET /v1/customers/cus_abc123
200 OK
{
  "id": "cus_abc123",
  "name": "Acme Corp",
  "email": "billing@acme.com",
  "metadata": {}
}

PUT /v1/customers/cus_abc123
{
  "name": "Acme Corporation",
  "email": "billing@acme.com",
  "metadata": {}
}
200 OK (full updated resource)
```

Rules:

- Server-assigned fields (`id`, `createdAt`, `updatedAt`) are silently ignored in PUT bodies. Do not reject them.
- The PUT body must include **all mutable fields**. A missing field means intent to clear/default, not "leave unchanged."
- Always return the full updated resource in the response.

For operations that don't fit "replace the whole thing," use action endpoints:

```
POST /v1/subscriptions/{id}/pause
POST /v1/subscriptions/{id}/resume
POST /v1/orders/{id}/refund         # Body: { "amount": 500, "reason": "duplicate" }
POST /v1/transfers/{id}/reverse
```

### Naming Conventions

| Element          | Convention                                   | Example                                 |
| ---------------- | -------------------------------------------- | --------------------------------------- |
| URL segments     | lowercase, hyphen-separated                  | `/v1/payment-methods`                   |
| JSON properties  | camelCase                                    | `paymentMethod`, `createdAt`            |
| Query parameters | camelCase                                    | `?pageSize=20&createdAfter=2024-01-01`  |
| Resource IDs     | prefixed with resource abbreviation          | `cus_`, `inv_`, `ord_`                  |
| Dates            | ISO 8601 UTC                                 | `2024-01-15T09:30:00Z`                  |
| Money            | integer in smallest currency unit + currency | `{ "amount": 1500, "currency": "usd" }` |

### HTTP Status Codes

Use consistently. Don't over-differentiate.

| Code                        | When                                                                   |
| --------------------------- | ---------------------------------------------------------------------- |
| `200 OK`                    | Successful GET, PUT, or action POST                                    |
| `201 Created`               | Successful resource-creating POST                                      |
| `204 No Content`            | Successful DELETE                                                      |
| `400 Bad Request`           | Malformed request, validation failure                                  |
| `401 Unauthorized`          | Missing or invalid authentication                                      |
| `403 Forbidden`             | Authenticated but insufficient permissions                             |
| `404 Not Found`             | Resource doesn't exist                                                 |
| `409 Conflict`              | State conflict (e.g., duplicate idempotency key with different params) |
| `422 Unprocessable Entity`  | Well-formed JSON but business rule violation                           |
| `429 Too Many Requests`     | Rate limited                                                           |
| `500 Internal Server Error` | Unhandled server error                                                 |

### Error Responses

Use RFC 9457 Problem Details as the base, extended with machine-readable error codes and field-level detail. ASP.NET Core has built-in support for this.

```json
{
  "type": "https://api.example.com/errors/validation-failed",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more fields failed validation.",
  "code": "validation_failed",
  "errors": [
    {
      "field": "email",
      "code": "invalid_format",
      "message": "Must be a valid email address."
    },
    {
      "field": "name",
      "code": "required",
      "message": "Name is required."
    }
  ]
}
```

Rules:

- `code` is a stable, machine-readable string. Clients switch on this, never on `message` or `detail`.
- `message` and `detail` are human-readable and may change without breaking clients.
- Include `errors` array for field-level validation; omit when not applicable.
- Use the same error shape across ALL endpoints. No endpoint-specific formats.

### List Responses and Pagination

Use cursor-based pagination. Never offset-based — it breaks under concurrent writes.

```json
{
  "data": [
    { "id": "cus_abc", "name": "Acme Corp" },
    { "id": "cus_def", "name": "Globex" }
  ],
  "hasMore": true,
  "cursor": "cus_def"
}
```

Query parameters:

- `pageSize` — items per page (default 20, max 100)
- `cursor` — opaque string from previous response; clients must not construct these
- `sortBy` — field to sort by
- `sortOrder` — `asc` or `desc`

The list envelope is always the same shape, even for zero results.

### Filtering

Flat query parameters with typed suffixes for range queries:

```
GET /v1/invoices?status=paid&createdAfter=2024-01-01&createdBefore=2024-06-01&customerId=cus_abc
```

- Exact match: bare field name (`?status=paid`)
- Range: `{field}After`, `{field}Before`
- Multiple values: comma-separated (`?status=paid,pending`)
- Do NOT use nested query syntax (`filter[status][eq]=paid`)

### Idempotency

POST endpoints that create resources must support idempotency keys:

```http
POST /v1/payments
Idempotency-Key: req_abc123
```

- Same key + same parameters: return the original response.
- Same key + different parameters: return `409 Conflict`.
- Keys expire after 24 hours.
- PUT and DELETE are inherently idempotent; no key needed.

### Versioning

Use URL path versioning: `/v1/`, `/v2/`.

Breaking changes require a new version:

- Removing or renaming a field
- Changing a field's type
- Changing error codes
- Changing URL structure

Non-breaking changes (no new version needed):

- Adding a new field to a response
- Adding a new endpoint
- Adding a new enum value to a response

## .NET Implementation Guidance

### DTOs

Use records with `required` for mandatory properties. Separate request and response DTOs.

```csharp
// Request DTO — only client-settable fields
public record CreateCustomerRequest
{
    public required string Name { get; init; }
    public required string Email { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
}

// PUT uses the same shape as create — all mutable fields required
public record UpdateCustomerRequest
{
    public required string Name { get; init; }
    public required string Email { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = [];
}

// Response DTO — includes server-assigned fields
public record CustomerResponse
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required DateTime CreatedAt { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = [];
}
```

Never expose the domain model or entity directly as an API response.

### List Response Envelope

```csharp
public record ListResponse<T>
{
    public required IReadOnlyList<T> Data { get; init; }
    public required bool HasMore { get; init; }
    public string? Cursor { get; init; }
}
```

### Route Registration

```csharp
var customers = app.MapGroup("/v1/customers");

customers.MapGet("/", CustomerEndpoints.List);
customers.MapPost("/", CustomerEndpoints.Create);
customers.MapGet("/{id}", CustomerEndpoints.Get);
customers.MapPut("/{id}", CustomerEndpoints.Update);
customers.MapDelete("/{id}", CustomerEndpoints.Delete);

// Action endpoints
customers.MapPost("/{id}/verify", CustomerEndpoints.Verify);
```

## Review Checklist

When designing or reviewing an API, verify:

- [ ] Resources are nouns, plural, kebab-case in URLs
- [ ] No PATCH endpoints; updates use PUT with full resource
- [ ] State transitions use action endpoints (`POST` with verb segment)
- [ ] Consistent error shape using Problem Details + `code` field
- [ ] List endpoints return `{ data, hasMore, cursor }` envelope
- [ ] Pagination is cursor-based, not offset-based
- [ ] IDs are prefixed with resource abbreviation
- [ ] Dates are ISO 8601 UTC
- [ ] Money is integer smallest-unit with explicit currency field
- [ ] POST creation endpoints support idempotency keys
- [ ] Filtering uses flat query parameters, not nested syntax
- [ ] Request and response DTOs are separate types
- [ ] PUT request DTOs require all mutable fields
- [ ] Server-assigned fields are excluded from request DTOs
- [ ] Versioned under `/v1/` path prefix
- [ ] No verbs in URL paths (except action endpoint suffixes)
- [ ] JSON properties are camelCase
