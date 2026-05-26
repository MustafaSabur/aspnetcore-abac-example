namespace AbacExample.Api.Endpoints;

public sealed record DocumentManagementContextResponse(
    Guid Id,
    bool CanUpdate,
    bool CanDelete);
