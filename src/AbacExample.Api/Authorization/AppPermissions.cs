namespace AbacExample.Api.Authorization;

public static class AppPermissions
{
    public const string DocumentCreate = "documents:create";
    public const string DocumentView = "documents:view";
    public const string DocumentEdit = "documents:edit";
    public const string DocumentArchive = "documents:archive";
    public const string DocumentManage = "documents:manage";

    public static readonly IReadOnlyCollection<string> All =
    [
        DocumentCreate,
        DocumentView,
        DocumentEdit,
        DocumentArchive,
        DocumentManage
    ];
}
