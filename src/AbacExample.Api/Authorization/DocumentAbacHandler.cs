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
            nameof(DocumentOperations.View) => CanView(user, document),
            nameof(DocumentOperations.Edit) => CanEdit(user, document),
            nameof(DocumentOperations.Archive) => CanArchive(user, document),
            nameof(DocumentOperations.Manage) => CanManage(user, document),
            _ => false
        };

        if (allowed)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private static bool CanView(ClaimsPrincipal user, Document document)
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

    private static bool CanEdit(ClaimsPrincipal user, Document document) =>
        !document.IsArchived &&
        (IsOwner(user, document) || CanUseRecordsManagerBreakGlass(user));

    private static bool CanArchive(ClaimsPrincipal user, Document document) =>
        !document.IsArchived &&
        (IsOwner(user, document) || CanUseRecordsManagerBreakGlass(user));

    private static bool CanManage(ClaimsPrincipal user, Document document) =>
        !document.IsArchived && CanUseRecordsManagerBreakGlass(user);

    private static bool IsOwner(ClaimsPrincipal user, Document document) =>
        user.UserId() == document.OwnerId;

    private static bool CanUseRecordsManagerBreakGlass(ClaimsPrincipal user) =>
        user.Claims.Any(claim =>
            claim.Type == AppClaims.Role &&
            string.Equals(claim.Value, AppRoles.RecordsManager, StringComparison.OrdinalIgnoreCase)) &&
        user.HasMfa();
}
