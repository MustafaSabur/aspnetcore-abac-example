namespace AbacExample.Api.Authorization;

public static class AppPermissions
{
    public const string CaseFileCreate = "case-files:create";
    public const string CaseFileView = "case-files:view";
    public const string CaseFileEdit = "case-files:edit";
    public const string CaseFileClose = "case-files:close";
    public const string CaseFileManage = "case-files:manage";

    public static readonly IReadOnlyCollection<string> All =
    [
        CaseFileCreate,
        CaseFileView,
        CaseFileEdit,
        CaseFileClose,
        CaseFileManage
    ];
}
