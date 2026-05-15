using AbacExample.Api.Authorization;
using AbacExample.Api.Domain;

namespace AbacExample.Api.Data;

public sealed class InMemoryAppAuthorizationProfileLoader : IAppAuthorizationProfileLoader
{
    private static readonly IReadOnlyDictionary<string, AppAuthorizationProfile> Profiles =
        new Dictionary<string, AppAuthorizationProfile>(StringComparer.OrdinalIgnoreCase)
        {
            ["patient-alice"] = new(
                SampleIds.UserAlice,
                "patient-alice",
                SampleIds.TenantNorth,
                AppUserKind.Patient,
                ClinicId: null,
                SampleIds.PatientAlice,
                ClinicianId: null,
                SensitivityLevel.Normal,
                CanSchedule: false,
                CanReadClinicAppointments: false,
                CanUsePlatformOverride: false,
                IsActive: true,
                ClaimsVersion: 1),

            ["clinician-ben"] = new(
                SampleIds.UserBen,
                "clinician-ben",
                SampleIds.TenantNorth,
                AppUserKind.Clinician,
                SampleIds.ClinicNorth,
                PatientId: null,
                SampleIds.ClinicianBen,
                SensitivityLevel.HighlySensitive,
                CanSchedule: false,
                CanReadClinicAppointments: true,
                CanUsePlatformOverride: false,
                IsActive: true,
                ClaimsVersion: 1),

            ["scheduler-cara"] = new(
                SampleIds.UserCara,
                "scheduler-cara",
                SampleIds.TenantNorth,
                AppUserKind.ClinicStaff,
                SampleIds.ClinicNorth,
                PatientId: null,
                ClinicianId: null,
                SensitivityLevel.Normal,
                CanSchedule: true,
                CanReadClinicAppointments: true,
                CanUsePlatformOverride: false,
                IsActive: true,
                ClaimsVersion: 1),

            ["platform-root"] = new(
                SampleIds.UserRoot,
                "platform-root",
                SampleIds.TenantNorth,
                AppUserKind.PlatformAdmin,
                ClinicId: null,
                PatientId: null,
                ClinicianId: null,
                SensitivityLevel.HighlySensitive,
                CanSchedule: false,
                CanReadClinicAppointments: false,
                CanUsePlatformOverride: true,
                IsActive: true,
                ClaimsVersion: 1),

            ["inactive-user"] = new(
                Guid.Parse("50000000-0000-0000-0000-000000000099"),
                "inactive-user",
                SampleIds.TenantNorth,
                AppUserKind.ClinicStaff,
                SampleIds.ClinicNorth,
                PatientId: null,
                ClinicianId: null,
                SensitivityLevel.Normal,
                CanSchedule: true,
                CanReadClinicAppointments: true,
                CanUsePlatformOverride: false,
                IsActive: false,
                ClaimsVersion: 1)
        };

    public Task<AppAuthorizationProfile?> LoadBySubjectAsync(
        string subject,
        CancellationToken cancellationToken = default)
    {
        Profiles.TryGetValue(subject, out var profile);
        return Task.FromResult(profile is { IsActive: true } ? profile : null);
    }

    public static IReadOnlyCollection<AppAuthorizationProfile> SampleProfiles =>
        Profiles.Values.Where(profile => profile.IsActive).ToArray();
}
