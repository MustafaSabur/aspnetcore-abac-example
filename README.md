# ASP.NET Core ABAC Example

This is a buildable .NET 10 ASP.NET Core Web API sample for ABAC-style authorization in a realistic case-files service. The external JWT stays small and identifies the subject with `sub`; the app loads its own authorization profile, expands fixed app roles into endpoint permissions, and then applies resource-specific rules for each case file.

The solution also includes `AbacExample.Authorization`, a reusable authorization library that can be shared by multiple ASP.NET Core services. The API project owns the case-file permissions, roles, EF Core data, endpoints, and ABAC rules.

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

$supervisor = dotnet user-jwts create --project $project --name case-supervisor-dev --output token
$mfaSupervisor = dotnet user-jwts create --project $project --name case-supervisor-dev --claim amr=mfa --output token
$agent = dotnet user-jwts create --project $project --name case-agent-dev --output token
$auditor = dotnet user-jwts create --project $project --name auditor-dev --output token
$outsideAuditor = dotnet user-jwts create --project $project --name outside-auditor-dev --output token
$unknown = dotnet user-jwts create --project $project --name unknown-dev --output token
```

Useful seeded case-file IDs:

- Public tenant A: `10000000-0000-0000-0000-000000000001`
- Confidential tenant A: `10000000-0000-0000-0000-000000000002`
- Agent-owned tenant A: `10000000-0000-0000-0000-000000000003`
- Supervisor-owned confidential tenant A: `10000000-0000-0000-0000-000000000004`
- Public tenant B: `20000000-0000-0000-0000-000000000001`

Expected results:

- No token: `401 Unauthorized`.
- `unknown-dev`: authenticated token, but no app profile, so protected endpoints return `403 Forbidden`.
- `auditor-dev`: can view non-confidential same-tenant case files; cannot create, edit, close, or manage case files.
- `case-agent-dev`: can create case files and edit assigned same-tenant case files; cannot manage confidential or non-owned case files.
- `case-supervisor-dev`: has close/manage endpoint permissions, but needs `amr=mfa` for break-glass access to confidential or non-owned case files.
- `case-supervisor-dev` with `amr=mfa`: can manage same-tenant confidential or non-owned case files.
- `outside-auditor-dev`: can view tenant B case files, but tenant A case files are forbidden by tenant mismatch.

## Project Shape

- `src/AbacExample.Authorization` contains reusable authorization primitives: app claim constants, profile loading contract, claims enrichment, current-user access, permission requirements, Minimal API permission extensions, and controller permission attributes.
- `src/AbacExample.Api/Authorization` contains case-file permission constants, fixed role mappings, EF-backed profile loading, and `CaseFileAbacHandler`.
- `src/AbacExample.Api/Data` contains `AppDbContext`, app-owned user/role/case-file entities, and Development seed data.
- `src/AbacExample.Api/Endpoints/CaseFileEndpoints.cs` maps Minimal API case-file routes and performs resource authorization.
- `src/AbacExample.Api/Controllers/CaseFilesController.cs` demonstrates the same permission model on a controller action.
- `src/AbacExample.Api/OpenApi` adds bearer security metadata to the generated OpenAPI document.

## Authorization Model

Endpoint permissions are service-owned case-file permissions:

| Endpoint kind | Permission |
| --- | --- |
| Create | `case-files:create` |
| View | `case-files:view` |
| Edit | `case-files:edit` |
| Close | `case-files:close` |
| Management context | `case-files:edit` OR `case-files:close` OR `case-files:manage` |

Role assignments live in app data and are tied to the external `sub`. Role names and permission mappings stay fixed in code:

- `case-agent`: create, view, edit
- `case-supervisor`: create, view, edit, close, manage
- `auditor`: view

`CaseFileAbacHandler` then enforces resource rules: tenant mismatch is a hard deny, owners can view/edit/close assigned case files when endpoint permissions allow it, auditors can view non-confidential same-tenant case files, and supervisor+MFA is the break-glass path for confidential or non-owned management actions.

Endpoints that need one of several permissions use `RequireAnyPermission(...)`. Do not stack multiple `.RequireAuthorization(...)` calls to model OR logic, because ASP.NET Core combines multiple authorization requirements as AND. The Minimal API and controller management-context endpoints both demonstrate the OR case.

## Real App Notes

Replace EF Core InMemory with the consuming app's database provider. Replace `dotnet user-jwts` with a real issuer by configuring JWT bearer settings such as `Authority` and `Audience`, and keep `MapInboundClaims = false` so claim names like `sub` and `amr` remain stable.
