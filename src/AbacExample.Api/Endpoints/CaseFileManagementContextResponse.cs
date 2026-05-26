namespace AbacExample.Api.Endpoints;

public sealed record CaseFileManagementContextResponse(
    Guid Id,
    bool CanEdit,
    bool CanClose,
    bool CanManage);
