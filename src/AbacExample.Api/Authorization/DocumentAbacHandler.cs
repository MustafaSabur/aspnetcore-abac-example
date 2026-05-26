using System.Security.Claims;
using AbacExample.Authorization;
using AbacExample.Api.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace AbacExample.Api.Authorization;

public sealed class DocumentAbacHandler(ILogger<DocumentAbacHandler> logger)
    : AuthorizationHandler<OperationAuthorizationRequirement, Document>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Document document)
    {
        var user = context.User;

        if (user.Identity?.IsAuthenticated != true ||
            !user.HasClaim(AppClaims.ProfileLoaded, BooleanClaimValues.True))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        if (user.TenantId() != document.TenantId)
        {
            logger.LogWarning(
                "ABAC denied tenant mismatch. UserId={UserId} DocumentId={DocumentId}",
                user.UserId(),
                document.Id);
            context.Fail();
            return Task.CompletedTask;
        }

        var allowed = requirement.Name switch
        {
            nameof(DocumentOperations.Read) => CanRead(user, document),
            nameof(DocumentOperations.Update) => CanUpdate(user, document),
            nameof(DocumentOperations.Delete) => CanDelete(user, document),
            _ => false
        };

        if (allowed)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private static bool CanRead(ClaimsPrincipal user, Document document)
    {
        if (IsOwner(user, document))
        {
            return true;
        }

        if (!document.IsConfidential)
        {
            return true;
        }

        return CanUseRecordsManagerBreakGlass(user);
    }

    private static bool CanUpdate(ClaimsPrincipal user, Document document) =>
        IsOwner(user, document) || CanUseRecordsManagerBreakGlass(user);

    private static bool CanDelete(ClaimsPrincipal user, Document document) =>
        IsOwner(user, document) || CanUseRecordsManagerBreakGlass(user);

    private static bool IsOwner(ClaimsPrincipal user, Document document) =>
        user.UserId() == document.OwnerId;

    private static bool CanUseRecordsManagerBreakGlass(ClaimsPrincipal user) =>
        user.Claims.Any(claim =>
            claim.Type == AppClaims.Role &&
            string.Equals(claim.Value, AppRoles.RecordsManager, StringComparison.OrdinalIgnoreCase)) &&
        user.HasMfa();
}
