using AbacExample.Authorization;
using AbacExample.Api.Authorization;
using AbacExample.Api.Data;
using AbacExample.Api.Endpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AbacExample.Api.Controllers;

[ApiController]
[Route("controller-documents")]
public sealed class DocumentsController(
    AppDbContext db,
    IAuthorizationService authorization,
    ICurrentUser currentUser)
    : ControllerBase
{
    [HttpGet("{id:guid}/management-context")]
    [RequireAnyPermission(
        AppPermissions.DocumentEdit,
        AppPermissions.DocumentArchive,
        AppPermissions.DocumentManage)]
    public async Task<ActionResult<DocumentManagementContextResponse>> GetManagementContext(
        Guid id,
        CancellationToken cancellationToken)
    {
        var document = await db.Documents
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (document is null)
        {
            return NotFound();
        }

        var principal = currentUser.Principal;
        var canEdit = principal.HasPermission(AppPermissions.DocumentEdit) &&
            (await authorization.AuthorizeAsync(principal, document, DocumentOperations.Edit)).Succeeded;
        var canArchive = principal.HasPermission(AppPermissions.DocumentArchive) &&
            (await authorization.AuthorizeAsync(principal, document, DocumentOperations.Archive)).Succeeded;
        var canManage = principal.HasPermission(AppPermissions.DocumentManage) &&
            (await authorization.AuthorizeAsync(principal, document, DocumentOperations.Manage)).Succeeded;

        if (!canEdit && !canArchive && !canManage)
        {
            return Forbid();
        }

        return Ok(new DocumentManagementContextResponse(document.Id, canEdit, canArchive, canManage));
    }
}
