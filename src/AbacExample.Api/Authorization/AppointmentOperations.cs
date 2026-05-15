using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace AbacExample.Api.Authorization;

public static class AppointmentOperations
{
    public static readonly OperationAuthorizationRequirement Read = new()
    {
        Name = nameof(Read)
    };

    public static readonly OperationAuthorizationRequirement Reschedule = new()
    {
        Name = nameof(Reschedule)
    };

    public static readonly OperationAuthorizationRequirement WriteClinicalNotes = new()
    {
        Name = nameof(WriteClinicalNotes)
    };

    public static readonly OperationAuthorizationRequirement Cancel = new()
    {
        Name = nameof(Cancel)
    };
}
