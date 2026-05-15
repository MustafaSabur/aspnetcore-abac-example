namespace AbacExample.Api.Data;

public static class SampleIds
{
    public static readonly Guid TenantNorth = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid TenantSouth = Guid.Parse("10000000-0000-0000-0000-000000000002");

    public static readonly Guid ClinicNorth = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid ClinicSouth = Guid.Parse("20000000-0000-0000-0000-000000000002");

    public static readonly Guid PatientAlice = Guid.Parse("30000000-0000-0000-0000-000000000001");
    public static readonly Guid PatientNora = Guid.Parse("30000000-0000-0000-0000-000000000002");
    public static readonly Guid PatientSam = Guid.Parse("30000000-0000-0000-0000-000000000003");

    public static readonly Guid ClinicianBen = Guid.Parse("40000000-0000-0000-0000-000000000001");
    public static readonly Guid ClinicianDana = Guid.Parse("40000000-0000-0000-0000-000000000002");

    public static readonly Guid UserAlice = Guid.Parse("50000000-0000-0000-0000-000000000001");
    public static readonly Guid UserBen = Guid.Parse("50000000-0000-0000-0000-000000000002");
    public static readonly Guid UserCara = Guid.Parse("50000000-0000-0000-0000-000000000003");
    public static readonly Guid UserRoot = Guid.Parse("50000000-0000-0000-0000-000000000004");

    public static readonly Guid NormalAppointment = Guid.Parse("60000000-0000-0000-0000-000000000001");
    public static readonly Guid SensitiveAppointment = Guid.Parse("60000000-0000-0000-0000-000000000002");
    public static readonly Guid OtherTenantAppointment = Guid.Parse("60000000-0000-0000-0000-000000000003");
}
