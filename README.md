# ASP.NET Core ABAC Example

This is a .NET 10 ASP.NET Core Web API sample that demonstrates a practical ABAC foundation:

- The external token stays small and only needs a subject (`sub`) plus optional authentication context such as `amr=mfa`.
- The app loads its own authorization profile for that subject.
- Resource-based authorization evaluates subject attributes, resource attributes, the operation, and runtime context.
- Tenant mismatch is a hard deny.
- Relationships are not permissions by themselves; explicit capabilities are required.
- Platform override is modeled as a break-glass path that requires MFA and audit logging.

## Run

```powershell
dotnet run --project .\src\AbacExample.Api\AbacExample.Api.csproj
```

In development, the app uses a sample authentication handler so the ABAC decisions can be tried without a real identity provider.

Use these headers:

```http
X-Sample-Subject: clinician-ben
X-Sample-Mfa: true
```

Useful endpoints:

- `GET /sample/users`
- `GET /sample/appointments`
- `GET /sample/whoami`
- `GET /appointments/{id}`
- `PATCH /appointments/{id}/reschedule`
- `PATCH /appointments/{id}/clinical-notes`
- `POST /appointments/{id}/cancel`

## Sample Subjects

| Subject | Meaning |
| --- | --- |
| `patient-alice` | Patient user who can read her own normal-sensitivity appointment. |
| `clinician-ben` | Assigned clinician with high clearance. Needs `X-Sample-Mfa: true` for highly sensitive notes. |
| `scheduler-cara` | Clinic staff user with scheduling permission and normal clearance. |
| `platform-root` | Platform admin with break-glass override. Requires `X-Sample-Mfa: true`. |
| `inactive-user` | Exists in the seed data but is inactive, so no app profile is loaded. |

## Example Resources

| Name | Id |
| --- | --- |
| Normal appointment | `60000000-0000-0000-0000-000000000001` |
| Highly sensitive appointment | `60000000-0000-0000-0000-000000000002` |
| Other tenant appointment | `60000000-0000-0000-0000-000000000003` |

## Design Rules

- Authentication proves who the external subject is.
- The app authorization profile decides whether that subject may use the app.
- Relationships such as patient, clinician, clinic, and tenant are attributes, not permissions by themselves.
- Capabilities such as scheduling or clinic-wide read access must be explicit.
- Tenant mismatch is a hard deny.
- Domain methods still enforce state invariants after authorization succeeds.
- Platform override is a break-glass path and must be audited.

## Production Notes

Set `SampleAuthentication:Enabled` to `false` outside local development and configure:

```json
{
  "Authentication": {
    "Authority": "https://issuer.example.com",
    "Audience": "api://abac-example"
  }
}
```

Replace the in-memory profile loader and appointment repository with database-backed implementations. The authorization shape should stay the same: load the app authorization profile, load the resource, authorize the operation, then call domain methods that enforce state invariants.
