using AbacExample.Api.Data.Entities;

namespace AbacExample.Api.Endpoints;

public sealed record CaseFileResponse(
    Guid Id,
    Guid TenantId,
    Guid OwnerId,
    bool IsConfidential,
    bool IsClosed,
    string? Summary)
{
    public static CaseFileResponse From(CaseFile caseFile) =>
        new(caseFile.Id, caseFile.TenantId, caseFile.OwnerId, caseFile.IsConfidential, caseFile.IsClosed, caseFile.Summary);
}
