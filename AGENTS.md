# Repository Guidelines

## Project Structure & Module Organization

This repository is a lean ASP.NET Core ABAC sample targeting .NET 10. The solution file is `AspNetCoreAbacExample.slnx`; the API project lives in `src/AbacExample.Api`.

- `Program.cs` wires authentication, EF Core, authorization policies, and endpoints.
- `Authorization/` contains permission constants, fixed role mappings, claims transformation, `ICurrentUser`, and the resource-based `DocumentAbacHandler`.
- `Data/` contains `AppDbContext`, app-owned identity/document entities, and Development seed data.
- `Endpoints/DocumentEndpoints.cs` keeps Minimal API route definitions and resource authorization calls.
- `OpenApi/` contains OpenAPI transformers for bearer-token metadata.
- `appsettings*.json` and `Properties/launchSettings.json` hold local configuration. Build output under `bin/` and `obj/` should not be edited.

## Build, Test, and Development Commands

Run commands from the repository root:

```powershell
dotnet restore .\AspNetCoreAbacExample.slnx
dotnet build .\AspNetCoreAbacExample.slnx
dotnet run --project .\src\AbacExample.Api\AbacExample.Api.csproj
```

`restore` downloads NuGet packages, `build` compiles the sample with nullable reference checks enabled, and `run` starts the local API using the project launch settings and EF Core InMemory provider.
Use `dotnet user-jwts create --project .\src\AbacExample.Api\AbacExample.Api.csproj --name reader-dev --output token` to create a local token, then paste it into Swagger UI at `/swagger`.

## Coding Style & Naming Conventions

Use standard C# formatting: four-space indentation, file-scoped namespaces, nullable-aware code, and implicit usings. Keep public types in files named after the type, for example `AppPermissions.cs` or `DocumentAbacHandler.cs`. Permission strings follow the `resource:action` pattern, such as `documents:read`. Keep role semantics fixed in code and user-role assignments in data.

## Testing Guidelines

There is currently no test project. Before handing off changes, run `dotnet build .\AspNetCoreAbacExample.slnx`. If tests are added, place them under `tests/`, name projects with a `.Tests` suffix, and use focused test names such as `DocumentAbacHandler_DeniesTenantMismatch`.

## Commit & Pull Request Guidelines

Recent history uses short imperative commit subjects, for example `Simplify ABAC example to a generic Document resource`. Keep commits focused and avoid bundling unrelated cleanup. Pull requests should describe the authorization behavior changed, list build/test results, and call out any changes to permissions, role mappings, claims, or endpoint policy requirements.

## Security & Configuration Tips

Do not commit real issuer, audience, database, or token values. This sample uses EF Core InMemory and `dotnet user-jwts` for local startup; replace them with a real provider and issuer in consuming applications. Preserve the separation between endpoint CRUD permissions and resource-specific ABAC rules.
