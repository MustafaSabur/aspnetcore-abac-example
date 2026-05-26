using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace AbacExample.Api.Authorization;

public static class DocumentOperations
{
    public static readonly OperationAuthorizationRequirement View = new() { Name = nameof(View) };
    public static readonly OperationAuthorizationRequirement Edit = new() { Name = nameof(Edit) };
    public static readonly OperationAuthorizationRequirement Archive = new() { Name = nameof(Archive) };
    public static readonly OperationAuthorizationRequirement Manage = new() { Name = nameof(Manage) };
}
