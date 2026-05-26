namespace AbacExample.Api.Endpoints;

public sealed record DocumentManagementContextResponse(
    Guid Id,
    bool CanEdit,
    bool CanArchive,
    bool CanManage);
