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
            .RequireAuthorization(AppPermissions.DocumentRead)
            .WithName("GetDocument");

        group.MapPut("/{id:guid}", UpdateDocument)
            .RequireAuthorization(AppPermissions.DocumentUpdate)
            .WithName("UpdateDocument");

        group.MapDelete("/{id:guid}", DeleteDocument)
            .RequireAuthorization(AppPermissions.DocumentDelete)
            .WithName("DeleteDocument");

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
            Content = request.Content
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

        var result = await authorization.AuthorizeAsync(currentUser.Principal, document, DocumentOperations.Read);

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

        var result = await authorization.AuthorizeAsync(currentUser.Principal, document, DocumentOperations.Update);

        if (!result.Succeeded)
        {
            return Results.Forbid();
        }

        document.Content = request.Content;
        await db.SaveChangesAsync(cancellationToken);

        return Results.Ok(DocumentResponse.From(document));
    }

    private static async Task<IResult> DeleteDocument(
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

        var result = await authorization.AuthorizeAsync(currentUser.Principal, document, DocumentOperations.Delete);

        if (!result.Succeeded)
        {
            return Results.Forbid();
        }

        db.Documents.Remove(document);
        await db.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}

public sealed record DocumentResponse(
    Guid Id,
    Guid TenantId,
    Guid OwnerId,
    bool IsConfidential,
    string? Content)
{
    public static DocumentResponse From(Document document) =>
        new(document.Id, document.TenantId, document.OwnerId, document.IsConfidential, document.Content);
}

public sealed record CreateDocumentRequest(bool IsConfidential, string? Content);

public sealed record UpdateDocumentRequest(string? Content);
