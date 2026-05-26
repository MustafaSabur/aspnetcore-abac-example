# Repository Guidelines

## Project Structure & Module Organization

This repository is a lean ASP.NET Core ABAC sample targeting .NET 10. The solution file is `AspNetCoreAbacExample.slnx`; the reusable authorization library lives in `src/AbacExample.Authorization`, and the runnable API lives in `src/AbacExample.Api`.

- `src/AbacExample.Authorization` contains reusable authorization primitives for multiple services: claim constants, profile contracts, optional claims transformation, current-user access, permission requirements, Minimal API permission extensions, and controller permission attributes.
- `src/AbacExample.Api/Program.cs` wires authentication, EF Core, authorization policies, controllers, Minimal API endpoints, and OpenAPI.
- `src/AbacExample.Api/Authorization` contains document permission constants, fixed role mappings, EF-backed profile loading, and `DocumentAbacHandler`.
- `src/AbacExample.Api/Data` contains `AppDbContext`, app-owned identity/document entities, and Development seed data.
- `src/AbacExample.Api/Endpoints/DocumentEndpoints.cs` keeps Minimal API route definitions and resource authorization calls.
- `src/AbacExample.Api/Controllers/DocumentsController.cs` shows controller-based permission usage.
- `src/AbacExample.Api/OpenApi` contains OpenAPI transformers for bearer-token metadata.

## Build, Test, and Development Commands

Run commands from the repository root:

```powershell
dotnet restore .\AspNetCoreAbacExample.slnx
dotnet build .\AspNetCoreAbacExample.slnx
dotnet run --project .\src\AbacExample.Api\AbacExample.Api.csproj
```

`restore` downloads NuGet packages, `build` compiles the sample with nullable reference checks enabled, and `run` starts the local API using the project launch settings and EF Core InMemory provider.
Use `dotnet user-jwts create --project .\src\AbacExample.Api\AbacExample.Api.csproj --name compliance-auditor-dev --output token` to create a local token, then paste it into Swagger UI at `/swagger`.

## Coding Style & Naming Conventions

Use standard C# formatting: four-space indentation, file-scoped namespaces, nullable-aware code, and implicit usings. Keep public types in files named after the type, for example `AppPermissions.cs` or `DocumentAbacHandler.cs`. Permission strings follow the `resource:action` pattern, such as `documents:read`. Keep reusable auth plumbing in `AbacExample.Authorization`; keep local profile enrichment opt-in; keep service-specific permissions, roles, data, and resource handlers in `AbacExample.Api`.

## Testing Guidelines

There is currently no test project. Before handing off changes, run `dotnet build .\AspNetCoreAbacExample.slnx`. If tests are added, place them under `tests/`, name projects with a `.Tests` suffix, and use focused test names such as `DocumentAbacHandler_DeniesTenantMismatch`.

## Commit & Pull Request Guidelines

Recent history uses short imperative commit subjects, for example `Simplify ABAC example to a generic Document resource`. Keep commits focused and avoid bundling unrelated cleanup. Pull requests should describe the authorization behavior changed, list build/test results, and call out any changes to permissions, role mappings, claims, endpoint policy requirements, or shared library contracts.

## Security & Configuration Tips

Do not commit real issuer, audience, database, or token values. This sample uses EF Core InMemory and `dotnet user-jwts` for local startup; replace them with a real provider and issuer in consuming applications. Preserve the separation between endpoint permissions and resource-specific ABAC rules.
