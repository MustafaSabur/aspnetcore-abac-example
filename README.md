# ASP.NET Core ABAC Example

This is a buildable .NET 10 ASP.NET Core Web API sample for ABAC-style authorization in a realistic documents service. The external JWT stays small and identifies the subject with `sub`; the app loads its own authorization profile, expands fixed app roles into endpoint permissions, and then applies resource-specific rules for each document.

The solution also includes `AbacExample.Authorization`, a reusable authorization library that can be shared by multiple ASP.NET Core services. The API project owns the document permissions, roles, EF Core data, endpoints, and ABAC rules.

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

$recordsManager = dotnet user-jwts create --project $project --name records-manager-dev --output token
$mfaRecordsManager = dotnet user-jwts create --project $project --name records-manager-dev --claim amr=mfa --output token
$documentAuthor = dotnet user-jwts create --project $project --name document-author-dev --output token
$complianceAuditor = dotnet user-jwts create --project $project --name compliance-auditor-dev --output token
$outsideAuditor = dotnet user-jwts create --project $project --name outside-compliance-auditor-dev --output token
$unknown = dotnet user-jwts create --project $project --name unknown-dev --output token
```

Useful seeded document IDs:

- Public tenant A: `10000000-0000-0000-0000-000000000001`
- Confidential tenant A: `10000000-0000-0000-0000-000000000002`
- Author-owned tenant A: `10000000-0000-0000-0000-000000000003`
- Records manager-owned confidential tenant A: `10000000-0000-0000-0000-000000000004`
- Public tenant B: `20000000-0000-0000-0000-000000000001`

Expected results:

- No token: `401 Unauthorized`.
- `unknown-dev`: authenticated token, but no app profile, so protected endpoints return `403 Forbidden`.
- `compliance-auditor-dev`: can read non-confidential same-tenant documents; cannot create, update, or delete documents.
- `document-author-dev`: can create documents and update assigned same-tenant documents; cannot delete confidential or non-owned documents.
- `records-manager-dev`: has delete endpoint permission, but needs `amr=mfa` for break-glass access to confidential or non-owned documents.
- `records-manager-dev` with `amr=mfa`: can update or delete same-tenant confidential or non-owned documents.
- `outside-compliance-auditor-dev`: can view tenant B documents, but tenant A documents are forbidden by tenant mismatch.

## Project Shape

- `src/AbacExample.Authorization` contains reusable authorization primitives: app claim constants, profile loading contract, claims enrichment, current-user access, permission requirements, Minimal API permission extensions, and controller permission attributes.
- `src/AbacExample.Api/Authorization` contains document permission constants, fixed role mappings, EF-backed profile loading, and `DocumentAbacHandler`.
- `src/AbacExample.Api/Data` contains `AppDbContext`, app-owned user/role/document entities, and Development seed data.
- `src/AbacExample.Api/Endpoints/DocumentEndpoints.cs` maps Minimal API document routes and performs resource authorization.
- `src/AbacExample.Api/Controllers/DocumentsController.cs` demonstrates the same permission model on a controller action.
- `src/AbacExample.Api/OpenApi` adds bearer security metadata to the generated OpenAPI document.

## Authorization Model

Endpoint permissions are service-owned document permissions:

| Endpoint kind | Permission |
| --- | --- |
| Create | `documents:create` |
| Read | `documents:read` |
| Update | `documents:update` |
| Delete | `documents:delete` |
| Management context | `documents:update` OR `documents:delete` |

Role assignments live in app data and are tied to the external `sub`. Role names and permission mappings stay fixed in code:

- `document-author`: create, read, update
- `records-manager`: create, read, update, delete
- `compliance-auditor`: read

`DocumentAbacHandler` then enforces resource rules: tenant mismatch is a hard deny, owners can read/update/delete assigned documents when endpoint permissions allow it, compliance auditors can read non-confidential same-tenant documents, and records-manager+MFA is the break-glass path for confidential or non-owned updates/deletes.

Endpoints that need one of several permissions use `RequireAnyPermission(...)`. Do not stack multiple `.RequireAuthorization(...)` calls to model OR logic, because ASP.NET Core combines multiple authorization requirements as AND. The Minimal API and controller management-context endpoints both demonstrate the OR case.

## Real App Notes

Replace EF Core InMemory with the consuming app's database provider. Replace `dotnet user-jwts` with a real issuer by configuring JWT bearer settings such as `Authority` and `Audience`, and keep `MapInboundClaims = false` so claim names like `sub` and `amr` remain stable.
