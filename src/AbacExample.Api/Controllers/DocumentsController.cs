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
    AbacExampleDbContext db,
    IAuthorizationService authorization,
    ICurrentUser currentUser)
    : ControllerBase
{
    [HttpGet("{id:guid}/management-context")]
    [RequireAnyPermission(
        DocumentPermissions.DocumentUpdate,
        DocumentPermissions.DocumentDelete)]
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
        var canUpdate = principal.HasPermission(DocumentPermissions.DocumentUpdate) &&
            (await authorization.AuthorizeAsync(principal, document, DocumentOperations.Update)).Succeeded;
        var canDelete = principal.HasPermission(DocumentPermissions.DocumentDelete) &&
            (await authorization.AuthorizeAsync(principal, document, DocumentOperations.Delete)).Succeeded;

        if (!canUpdate && !canDelete)
        {
            return Forbid();
        }

        return Ok(new DocumentManagementContextResponse(document.Id, canUpdate, canDelete));
    }
}
