using Microsoft.AspNetCore.Authorization;

namespace AbacExample.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequireAnyPermissionAttribute : AuthorizeAttribute
{
    public RequireAnyPermissionAttribute(params string[] permissions)
    {
        Policy = PermissionPolicyNames.ForAny(permissions);
    }
}
