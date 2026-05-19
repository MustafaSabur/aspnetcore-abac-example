using System.Security.Claims;
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
        if (user.UserId() == document.OwnerId)
        {
            return true;
        }

        return !document.IsConfidential || user.HasMfa();
    }

    private static bool CanUpdate(ClaimsPrincipal user, Document document) =>
        user.UserId() == document.OwnerId;

    private static bool CanDelete(ClaimsPrincipal user, Document document) =>
        user.UserId() == document.OwnerId;
}
