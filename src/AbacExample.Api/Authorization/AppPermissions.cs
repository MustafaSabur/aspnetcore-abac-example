namespace AbacExample.Api.Authorization;

public static class AppPermissions
{
    public const string DocumentCreate = "documents:create";
    public const string DocumentRead = "documents:read";
    public const string DocumentUpdate = "documents:update";
    public const string DocumentDelete = "documents:delete";

    public static readonly IReadOnlyCollection<string> All =
    [
        DocumentCreate,
        DocumentRead,
        DocumentUpdate,
        DocumentDelete
    ];
}
