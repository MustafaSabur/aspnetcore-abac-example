namespace AbacExample.Api.Data.Entities;

public sealed class CaseFile
{
    public required Guid Id { get; init; }
    public required Guid TenantId { get; init; }
    public required Guid OwnerId { get; init; }
    public required bool IsConfidential { get; init; }
    public bool IsClosed { get; set; }
    public string? Summary { get; set; }
}
