namespace AbacExample.Api.Data.Entities;

public sealed class AppUserRole
{
    public Guid AppUserId { get; init; }
    public AppUser AppUser { get; init; } = null!;
    public required string RoleName { get; init; }
}
