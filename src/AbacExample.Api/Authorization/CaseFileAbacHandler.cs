using System.Security.Claims;
using AbacExample.Authorization;
using AbacExample.Api.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace AbacExample.Api.Authorization;

public sealed class CaseFileAbacHandler(ILogger<CaseFileAbacHandler> logger)
    : AuthorizationHandler<OperationAuthorizationRequirement, CaseFile>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        CaseFile caseFile)
    {
        var user = context.User;

        if (user.Identity?.IsAuthenticated != true ||
            !user.HasClaim(AppClaims.ProfileLoaded, BooleanClaimValues.True))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        if (user.TenantId() != caseFile.TenantId)
        {
            logger.LogWarning(
                "ABAC denied tenant mismatch. UserId={UserId} CaseFileId={CaseFileId}",
                user.UserId(),
                caseFile.Id);
            context.Fail();
            return Task.CompletedTask;
        }

        var allowed = requirement.Name switch
        {
            nameof(CaseFileOperations.View) => CanView(user, caseFile),
            nameof(CaseFileOperations.Edit) => CanEdit(user, caseFile),
            nameof(CaseFileOperations.Close) => CanClose(user, caseFile),
            nameof(CaseFileOperations.Manage) => CanManage(user, caseFile),
            _ => false
        };

        if (allowed)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private static bool CanView(ClaimsPrincipal user, CaseFile caseFile)
    {
        if (IsOwner(user, caseFile))
        {
            return true;
        }

        if (!caseFile.IsConfidential)
        {
            return true;
        }

        return CanUseSupervisorBreakGlass(user);
    }

    private static bool CanEdit(ClaimsPrincipal user, CaseFile caseFile) =>
        !caseFile.IsClosed &&
        (IsOwner(user, caseFile) || CanUseSupervisorBreakGlass(user));

    private static bool CanClose(ClaimsPrincipal user, CaseFile caseFile) =>
        !caseFile.IsClosed &&
        (IsOwner(user, caseFile) || CanUseSupervisorBreakGlass(user));

    private static bool CanManage(ClaimsPrincipal user, CaseFile caseFile) =>
        !caseFile.IsClosed && CanUseSupervisorBreakGlass(user);

    private static bool IsOwner(ClaimsPrincipal user, CaseFile caseFile) =>
        user.UserId() == caseFile.OwnerId;

    private static bool CanUseSupervisorBreakGlass(ClaimsPrincipal user) =>
        user.Claims.Any(claim =>
            claim.Type == AppClaims.Role &&
            string.Equals(claim.Value, AppRoles.CaseSupervisor, StringComparison.OrdinalIgnoreCase)) &&
        user.HasMfa();
}
