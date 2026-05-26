using AbacExample.Authorization;
using AbacExample.Api.Authorization;
using AbacExample.Api.Data;
using AbacExample.Api.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AbacExample.Api.Endpoints;

public static class CaseFileEndpoints
{
    public static IEndpointRouteBuilder MapCaseFileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/case-files")
            .RequireAuthorization()
            .WithTags("CaseFiles");

        group.MapPost("/", CreateCaseFile)
            .RequireAuthorization(AppPermissions.CaseFileCreate)
            .WithName("CreateCaseFile");

        group.MapGet("/{id:guid}", GetCaseFile)
            .RequireAuthorization(AppPermissions.CaseFileView)
            .WithName("GetCaseFile");

        group.MapPut("/{id:guid}", UpdateCaseFile)
            .RequireAuthorization(AppPermissions.CaseFileEdit)
            .WithName("UpdateCaseFile");

        group.MapPost("/{id:guid}/close", CloseCaseFile)
            .RequireAuthorization(AppPermissions.CaseFileClose)
            .WithName("CloseCaseFile");

        group.MapGet("/{id:guid}/management-context", GetCaseFileManagementContext)
            .RequireAnyPermission(
                AppPermissions.CaseFileEdit,
                AppPermissions.CaseFileClose,
                AppPermissions.CaseFileManage)
            .WithName("GetCaseFileManagementContext");

        return app;
    }

    private static async Task<IResult> CreateCaseFile(
        CreateCaseFileRequest request,
        AppDbContext db,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var caseFile = new CaseFile
        {
            Id = Guid.NewGuid(),
            TenantId = currentUser.TenantId,
            OwnerId = currentUser.UserId,
            IsConfidential = request.IsConfidential,
            Summary = request.Summary
        };

        db.CaseFiles.Add(caseFile);
        await db.SaveChangesAsync(cancellationToken);

        return Results.Created($"/case-files/{caseFile.Id}", CaseFileResponse.From(caseFile));
    }

    private static async Task<IResult> GetCaseFile(
        Guid id,
        AppDbContext db,
        IAuthorizationService authorization,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var caseFile = await db.CaseFiles
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (caseFile is null)
        {
            return Results.NotFound();
        }

        var result = await authorization.AuthorizeAsync(currentUser.Principal, caseFile, CaseFileOperations.View);

        if (!result.Succeeded)
        {
            return Results.Forbid();
        }

        return Results.Ok(CaseFileResponse.From(caseFile));
    }

    private static async Task<IResult> UpdateCaseFile(
        Guid id,
        UpdateCaseFileRequest request,
        AppDbContext db,
        IAuthorizationService authorization,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var caseFile = await db.CaseFiles
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (caseFile is null)
        {
            return Results.NotFound();
        }

        var result = await authorization.AuthorizeAsync(currentUser.Principal, caseFile, CaseFileOperations.Edit);

        if (!result.Succeeded)
        {
            return Results.Forbid();
        }

        caseFile.Summary = request.Summary;
        await db.SaveChangesAsync(cancellationToken);

        return Results.Ok(CaseFileResponse.From(caseFile));
    }

    private static async Task<IResult> CloseCaseFile(
        Guid id,
        AppDbContext db,
        IAuthorizationService authorization,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var caseFile = await db.CaseFiles
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (caseFile is null)
        {
            return Results.NotFound();
        }

        var result = await authorization.AuthorizeAsync(currentUser.Principal, caseFile, CaseFileOperations.Close);

        if (!result.Succeeded)
        {
            return Results.Forbid();
        }

        caseFile.IsClosed = true;
        await db.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> GetCaseFileManagementContext(
        Guid id,
        AppDbContext db,
        IAuthorizationService authorization,
        ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var caseFile = await db.CaseFiles
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (caseFile is null)
        {
            return Results.NotFound();
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
            return Results.Forbid();
        }

        return Results.Ok(new CaseFileManagementContextResponse(caseFile.Id, canEdit, canClose, canManage));
    }
}
