using AbacExample.Api.Data.Entities;

namespace AbacExample.Api.Endpoints;

public sealed record DocumentResponse(
    Guid Id,
    Guid TenantId,
    Guid OwnerId,
    bool IsConfidential,
    bool IsArchived,
    string? Summary)
{
    public static DocumentResponse From(Document document) =>
        new(document.Id, document.TenantId, document.OwnerId, document.IsConfidential, document.IsArchived, document.Summary);
}
