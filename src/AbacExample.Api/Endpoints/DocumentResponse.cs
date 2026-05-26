using AbacExample.Api.Data.Entities;

namespace AbacExample.Api.Endpoints;

public sealed record DocumentResponse(
    Guid Id,
    Guid TenantId,
    Guid OwnerId,
    bool IsConfidential,
    string? Content)
{
    public static DocumentResponse From(Document document) =>
        new(document.Id, document.TenantId, document.OwnerId, document.IsConfidential, document.Content);
}
