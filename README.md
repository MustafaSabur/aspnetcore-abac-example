# ASP.NET Core ABAC Example

This is a buildable .NET 10 ASP.NET Core Web API example for a future ABAC implementation. It intentionally does not include fake authentication, seed data, sample endpoints, or in-memory persistence.

The goal is to show the authorization shape:

- The external token stays small and provides identity-provider facts such as `sub`, plus optional authentication context such as `amr=mfa`.
- The application loads its own authorization profile for that subject.
- Resource-based authorization evaluates subject attributes, resource attributes, the operation, and runtime context.
- Tenant mismatch is a hard deny.
- Relationships are attributes, not permissions by themselves.
- Capabilities such as scheduling and clinic-wide read access are explicit.
- Platform override is modeled as a break-glass path that requires MFA and audit logging.

## Build

```powershell
dotnet build .\AspNetCoreAbacExample.slnx
```

## Project Shape

- `Authorization/AppClaims.cs` defines identity-provider and app-owned authorization claim names.
- `Authorization/AppAuthorizationProfile.cs` defines the app-owned authorization profile and loader port.
- `Authorization/AppClaimsTransformation.cs` projects the loaded app profile onto the current request principal.
- `Authorization/AuthorizationAudit.cs` contains the resource-based ABAC handler and audit port.
- `Authorization/CurrentUser.cs` and `Authorization/ICurrentUser.cs` provide a typed current-user wrapper.
- `Domain/Appointment.cs` keeps domain state invariants after authorization succeeds.
- `Endpoints/AppointmentEndpoints.cs` shows where resources are loaded before calling `IAuthorizationService`.
- `Data/IAppointmentRepository.cs` is the persistence boundary the future app should implement.

## Required Infrastructure For A Real App

Add concrete implementations in the consuming application:

```csharp
builder.Services.AddScoped<IAppAuthorizationProfileLoader, DbAppAuthorizationProfileLoader>();
builder.Services.AddScoped<IAppointmentRepository, DbAppointmentRepository>();
```

Configure JWT bearer authentication:

```json
{
  "Authentication": {
    "Authority": "https://issuer.example.com",
    "Audience": "api://abac-example"
  }
}
```

## Design Rules

- Authentication proves who the external subject is.
- The app authorization profile decides whether that subject may use the app.
- Relationships such as patient, clinician, clinic, and tenant are attributes, not permissions by themselves.
- Capabilities such as scheduling or clinic-wide read access must be explicit.
- Tenant mismatch is a hard deny.
- Domain methods still enforce state invariants after authorization succeeds.
- Platform override is a break-glass path and must be audited.
