using AbacExample.Authorization;
using AbacExample.Api.Authorization;
using AbacExample.Api.Data;
using AbacExample.Api.Endpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AbacExample.Api.Controllers;

[ApiController]
[Route("controller-case-files")]
public sealed class CaseFilesController(
    AppDbContext db,
    IAuthorizationService authorization,
    ICurrentUser currentUser)
    : ControllerBase
{
    [HttpGet("{id:guid}/management-context")]
    [RequireAnyPermission(
        AppPermissions.CaseFileEdit,
        AppPermissions.CaseFileClose,
        AppPermissions.CaseFileManage)]
    public async Task<ActionResult<CaseFileManagementContextResponse>> GetManagementContext(
        Guid id,
        CancellationToken cancellationToken)
    {
        var caseFile = await db.CaseFiles
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (caseFile is null)
        {
            return NotFound();
        }

        var principal = currentUser.Principal;
        var canEdit = principal.HasPermission(AppPermissions.CaseFileEdit) &&
            (await authorization.AuthorizeAsync(principal, caseFile, CaseFileOperations.Edit)).Succeeded;
        var canClose = principal.HasPermission(AppPermissions.CaseFileClose) &&
            (await authorization.AuthorizeAsync(principal, caseFile, CaseFileOperations.Close)).Succeeded;
        var canManage = principal.HasPermission(AppPermissions.CaseFileManage) &&
            (await authorization.AuthorizeAsync(principal, caseFile, CaseFileOperations.Manage)).Succeeded;

        if (!canEdit && !canClose && !canManage)
        {
            return Forbid();
        }

        return Ok(new CaseFileManagementContextResponse(caseFile.Id, canEdit, canClose, canManage));
    }
}
