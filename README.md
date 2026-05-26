# ASP.NET Core ABAC Example

This is a buildable .NET 10 ASP.NET Core Web API sample for ABAC-style authorization. The external JWT stays small and identifies the subject with `sub`; the app loads its own authorization profile, expands fixed app roles into endpoint permissions, and then applies resource-specific rules for each document.

The sample uses EF Core InMemory and Development seed data so it can run locally without database setup.

## Run

```powershell
dotnet build .\AspNetCoreAbacExample.slnx
dotnet run --project .\src\AbacExample.Api\AbacExample.Api.csproj
```

Open Swagger UI at `https://localhost:53834/swagger`. The OpenAPI document is available at `https://localhost:53834/openapi/v1.json`. Both are anonymous in Development so you can paste a JWT before calling protected endpoints.

## Local JWTs

Create local development tokens with `dotnet user-jwts`. Paste the raw token value into Swagger UI's `BearerAuth` authorization field.

```powershell
$project = ".\src\AbacExample.Api\AbacExample.Api.csproj"

$admin = dotnet user-jwts create --project $project --name admin-dev --output token
$mfaAdmin = dotnet user-jwts create --project $project --name admin-dev --claim amr=mfa --output token
$editor = dotnet user-jwts create --project $project --name editor-dev --output token
$reader = dotnet user-jwts create --project $project --name reader-dev --output token
$mfaReader = dotnet user-jwts create --project $project --name reader-dev --claim amr=mfa --output token
$outsider = dotnet user-jwts create --project $project --name outsider-dev --output token
$unknown = dotnet user-jwts create --project $project --name unknown-dev --output token
```

Useful seeded document IDs:

- Public tenant A: `10000000-0000-0000-0000-000000000001`
- Confidential tenant A: `10000000-0000-0000-0000-000000000002`
- Reader-owned tenant A: `10000000-0000-0000-0000-000000000003`
- Admin-owned confidential tenant A: `10000000-0000-0000-0000-000000000004`
- Public tenant B: `20000000-0000-0000-0000-000000000001`

Expected results:

- No token: `401 Unauthorized`.
- `unknown-dev`: authenticated token, but no app profile, so protected endpoints return `403 Forbidden`.
- `reader-dev`: can read public same-tenant documents; cannot create, update, delete, or read confidential non-owned documents.
- `reader-dev` with `amr=mfa`: still cannot read confidential non-owned documents because break-glass is admin-only.
- `editor-dev`: can create documents and update documents it owns; it cannot update another user's document.
- `editor-dev`: can call `GET /documents/{id}/management-context` for documents it can update, even though it does not have delete permission.
- `admin-dev` with `amr=mfa`: can use break-glass access on same-tenant confidential or non-owned documents.
- `outsider-dev`: can read tenant B documents, but tenant A documents are forbidden by tenant mismatch.

## Project Shape

- `Authorization/` defines identity-provider claim names, app claim names, fixed roles, permission policies, current-user access, and `DocumentAbacHandler`.
- `Data/` contains `AppDbContext`, app-owned user/role/document entities, and Development seed data.
- `Endpoints/DocumentEndpoints.cs` maps Minimal API document routes and performs resource authorization.
- `OpenApi/` adds bearer security metadata to the generated OpenAPI document.

## Authorization Model

Endpoint permissions are coarse CRUD gates:

| Endpoint kind | Permission |
| --- | --- |
| Create | `documents:create` |
| Read | `documents:read` |
| Update | `documents:update` |
| Delete | `documents:delete` |
| Management context | `documents:update` OR `documents:delete` |

Role assignments live in app data and are tied to the external `sub`. Role names and permission mappings stay fixed in code:

- `admin`: create, read, update, delete
- `editor`: create, read, update
- `reader`: read

`DocumentAbacHandler` then enforces resource rules: tenant mismatch is a hard deny, owners can read/update/delete their own documents when endpoint permissions allow it, non-owners can read public same-tenant documents, and admin+MFA is the only break-glass path for confidential or non-owned updates/deletes.

Endpoints that need one of several permissions use `RequireAnyPermission(...)`. Do not stack multiple `.RequireAuthorization(...)` calls to model OR logic, because ASP.NET Core combines multiple authorization requirements as AND. `GET /documents/{id}/management-context` demonstrates the OR case: the endpoint admits callers with either update or delete permission, then returns only the actions that also pass the document resource rules.

## Real App Notes

Replace EF Core InMemory with the consuming app's database provider. Replace `dotnet user-jwts` with a real issuer by configuring JWT bearer settings such as `Authority` and `Audience`, and keep `MapInboundClaims = false` so claim names like `sub` and `amr` remain stable.
