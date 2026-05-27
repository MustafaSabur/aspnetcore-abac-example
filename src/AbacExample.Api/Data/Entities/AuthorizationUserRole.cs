namespace AbacExample.Api.Data.Entities;

public sealed class AuthorizationUserRole
{
    public Guid AuthorizationUserId { get; init; }
    public AuthorizationUser AuthorizationUser { get; init; } = null!;
    public required string RoleName { get; init; }
}
