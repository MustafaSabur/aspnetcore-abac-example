using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace AbacExample.Api.Authorization;

public static class CaseFileOperations
{
    public static readonly OperationAuthorizationRequirement View = new() { Name = nameof(View) };
    public static readonly OperationAuthorizationRequirement Edit = new() { Name = nameof(Edit) };
    public static readonly OperationAuthorizationRequirement Close = new() { Name = nameof(Close) };
    public static readonly OperationAuthorizationRequirement Manage = new() { Name = nameof(Manage) };
}
