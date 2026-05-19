namespace AbacExample.Api.Data.Entities;

public sealed class Document
{
    public required Guid Id { get; init; }
    public required Guid TenantId { get; init; }
    public required Guid OwnerId { get; init; }
    public required bool IsConfidential { get; init; }
    public string? Content { get; set; }
}
