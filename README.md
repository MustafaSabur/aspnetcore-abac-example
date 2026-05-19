# ASP.NET Core ABAC Example

This is a buildable .NET 10 ASP.NET Core Web API example for a future ABAC implementation. It intentionally does not include fake authentication or seed data. The app uses EF Core InMemory by default so the sample can start locally without database setup.

The goal is to show the authorization shape:

- The external token stays small and provides identity-provider facts such as `sub`, plus optional authentication context such as `amr=mfa`.
- The application loads its own authorization profile for that subject.
- Resource-based authorization evaluates subject attributes, resource attributes, the operation, and runtime context.
- Endpoint policies enforce coarse CRUD permissions before resource-specific ABAC checks run.
- Tenant mismatch is a hard deny.
- Relationships are attributes, not permissions by themselves.
- Role assignments are stored in the app database and tied to `sub` through `AppUser`; a user can have multiple roles.
- Role permissions are fixed in code so role semantics cannot drift through database data.
- Platform override is modeled as a break-glass path that requires MFA.

## Build

```powershell
dotnet build .\AspNetCoreAbacExample.slnx
```

## Project Shape

- `Authorization/AppClaims.cs` defines identity-provider and app-owned authorization claim names.
- `Authorization/AppAuthorizationProfile.cs` defines the app-owned authorization profile and loader port.
- `Authorization/AppClaimsTransformation.cs` projects the loaded app profile onto the current request principal.
- `Authorization/AppPermissions.cs` defines CRUD permissions and registers each permission as a policy with the same name.
- `Authorization/AppRoles.cs` defines fixed role names and their permission sets.
- `Authorization/AppointmentAbacHandler.cs` contains the resource-based ABAC handler.
- `Authorization/CurrentUser.cs` and `Authorization/ICurrentUser.cs` provide a typed current-user wrapper.
- `Data/AppIdentityModels.cs` contains app-owned user and user-role assignment entities.
- `Data/DbAppAuthorizationProfileLoader.cs` loads role assignments for the authenticated `sub` and expands them to permissions in code.
- `Domain/Appointment.cs` keeps domain state invariants after authorization succeeds.
- `Endpoints/AppointmentEndpoints.cs` shows loading resources through EF Core before calling `IAuthorizationService`.
- `Data/AppDbContext.cs` is the EF Core data boundary. The sample uses EF Core InMemory by default; a real app should replace it with its database provider.

## Endpoint Permissions

Appointment endpoints use named policies for coarse permissions:

| Endpoint kind | Permission |
| --- | --- |
| Create | `appointments:create` |
| Read | `appointments:read` |
| Update | `appointments:update` |
| Delete/cancel | `appointments:delete` |

The example also includes an endpoint that accepts **any** of multiple permissions without adding another named policy:

```csharp
.RequireAnyPermission(
    AppPermissions.AppointmentRead,
    AppPermissions.AppointmentUpdate)
```

## App Roles

Role assignments are stored in the app database and assigned to app users resolved by external `sub`. A user may have multiple roles, and the authorization profile receives the union of all permissions for those fixed roles.

Defined role names:

- `admin`
- `read-only`
- `scheduler`
- `clinician`

The example keeps role names and permission sets in code so the application has a stable authorization vocabulary, while only user-role assignments live in the app database.

## Required Infrastructure For A Real App

Replace the sample EF Core InMemory registration with the database provider for the consuming application:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Replace the sample InMemory provider with your database provider here.
});
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
- Endpoint permissions decide whether the subject can reach a class of action.
- `AppointmentAbacHandler` decides whether the subject can perform that action on a specific appointment.
- Tenant mismatch is a hard deny.
- Domain methods still enforce state invariants after authorization succeeds.
- Platform override is a break-glass path and should stay explicit.
