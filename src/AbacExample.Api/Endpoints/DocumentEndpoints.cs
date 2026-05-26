using AbacExample.Authorization;
using AbacExample.Api.Authorization;
using AbacExample.Api.Data;
using AbacExample.Api.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AbacExample.Api.Endpoints;

public static class DocumentEndpoints
{
    public static IEndpointRouteBuilder MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/documents")
            .RequireAuthorization()
            .WithTags("Documents");

        group.MapPost("/", CreateDocument)
            .RequireAuthorization(AppPermissions.DocumentCreate)
            .WithName("CreateDocument");

        group.MapGet("/{id:guid}", GetDocument)
            .RequireAuthorization(AppPermissions.DocumentView)
            .WithName("GetDocument");

        group.MapPut("/{id:guid}", UpdateDocument)
            .RequireAuthorization(AppPermissions.DocumentEdit)
            .WithName("UpdateDocument");

        group.MapPost("/{id:guid}/archive", ArchiveDocument)
            .RequireAuthorization(AppPermissions.DocumentArchive)
            .WithName("ArchiveDocument");

        group.MapGet("/{id:guid}/management-context", GetDocumentManagementContext)
            .RequireAnyPermission(
                AppPermissions.DocumentEdit,
                AppPermissions.DocumentArchive,
                AppPermissions.DocumentManage)
            .WithName("GetDocumentManagementContext");

        return app;
    }

    private static async Task<IResult> CreateDocument(
        CreateDocumentRequest request,
        AppDbContext db,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var document = new Document
        {
            Id = Guid.NewGuid(),
            TenantId = currentUser.TenantId,
            OwnerId = currentUser.UserId,
            IsConfidential = request.IsConfidential,
            Summary = request.Summary
        };

        db.Documents.Add(document);
        await db.SaveChangesAsync(cancellationToken);

        return Results.Created($"/documents/{document.Id}", DocumentResponse.From(document));
    }

    private static async Task<IResult> GetDocument(
        Guid id,
        AppDbContext db,
        IAuthorizationService authorization,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var document = await db.Documents
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (document is null)
        {
            return Results.NotFound();
        }

        var result = await authorization.AuthorizeAsync(currentUser.Principal, document, DocumentOperations.View);

        if (!result.Succeeded)
        {
            return Results.Forbid();
        }

        return Results.Ok(DocumentResponse.From(document));
    }

    private static async Task<IResult> UpdateDocument(
        Guid id,
        UpdateDocumentRequest request,
        AppDbContext db,
        IAuthorizationService authorization,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var document = await db.Documents
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (document is null)
        {
            return Results.NotFound();
        }

        var result = await authorization.AuthorizeAsync(currentUser.Principal, document, DocumentOperations.Edit);

        if (!result.Succeeded)
        {
            return Results.Forbid();
        }

        document.Summary = request.Summary;
        await db.SaveChangesAsync(cancellationToken);

        return Results.Ok(DocumentResponse.From(document));
    }

    private static async Task<IResult> ArchiveDocument(
        Guid id,
        AppDbContext db,
        IAuthorizationService authorization,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var document = await db.Documents
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (document is null)
        {
            return Results.NotFound();
        }

        var result = await authorization.AuthorizeAsync(currentUser.Principal, document, DocumentOperations.Archive);

        if (!result.Succeeded)
        {
            return Results.Forbid();
        }

        document.IsArchived = true;
        await db.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> GetDocumentManagementContext(
        Guid id,
        AppDbContext db,
        IAuthorizationService authorization,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var document = await db.Documents
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (document is null)
        {
            return Results.NotFound();
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
            return Results.Forbid();
        }

        return Results.Ok(new DocumentManagementContextResponse(document.Id, canEdit, canArchive, canManage));
    }
}
