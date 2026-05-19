using AbacExample.Api.Authorization;
using AbacExample.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AbacExample.Api.Data;

public static class DevelopmentDataSeeder
{
    public const string AdminSubject = "admin-dev";
    public const string EditorSubject = "editor-dev";
    public const string ReaderSubject = "reader-dev";
    public const string OutsiderSubject = "outsider-dev";

    public static readonly Guid TenantAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid TenantBId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static readonly Guid AdminUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid EditorUserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid ReaderUserId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    public static readonly Guid OutsiderUserId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    public static readonly Guid PublicDocumentId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid ConfidentialDocumentId = Guid.Parse("10000000-0000-0000-0000-000000000002");
    public static readonly Guid ReaderOwnedDocumentId = Guid.Parse("10000000-0000-0000-0000-000000000003");
    public static readonly Guid AdminOwnedConfidentialDocumentId = Guid.Parse("10000000-0000-0000-0000-000000000004");
    public static readonly Guid OutsiderDocumentId = Guid.Parse("20000000-0000-0000-0000-000000000001");

    public static async Task SeedDevelopmentDataAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await db.AppUsers.AnyAsync())
        {
            return;
        }

        var admin = CreateUser(AdminUserId, AdminSubject, TenantAId);
        var editor = CreateUser(EditorUserId, EditorSubject, TenantAId);
        var reader = CreateUser(ReaderUserId, ReaderSubject, TenantAId);
        var outsider = CreateUser(OutsiderUserId, OutsiderSubject, TenantBId);

        db.AppUsers.AddRange(admin, editor, reader, outsider);
        db.AppUserRoles.AddRange(
            CreateRole(admin.Id, AppRoles.Admin),
            CreateRole(editor.Id, AppRoles.Editor),
            CreateRole(reader.Id, AppRoles.Reader),
            CreateRole(outsider.Id, AppRoles.Reader));

        db.Documents.AddRange(
            new Document
            {
                Id = PublicDocumentId,
                TenantId = TenantAId,
                OwnerId = EditorUserId,
                IsConfidential = false,
                Content = "Tenant A public document owned by editor-dev."
            },
            new Document
            {
                Id = ConfidentialDocumentId,
                TenantId = TenantAId,
                OwnerId = EditorUserId,
                IsConfidential = true,
                Content = "Tenant A confidential document owned by editor-dev."
            },
            new Document
            {
                Id = ReaderOwnedDocumentId,
                TenantId = TenantAId,
                OwnerId = ReaderUserId,
                IsConfidential = false,
                Content = "Tenant A public document owned by reader-dev."
            },
            new Document
            {
                Id = AdminOwnedConfidentialDocumentId,
                TenantId = TenantAId,
                OwnerId = AdminUserId,
                IsConfidential = true,
                Content = "Tenant A confidential document owned by admin-dev."
            },
            new Document
            {
                Id = OutsiderDocumentId,
                TenantId = TenantBId,
                OwnerId = OutsiderUserId,
                IsConfidential = false,
                Content = "Tenant B document owned by outsider-dev."
            });

        await db.SaveChangesAsync();
    }

    private static AppUser CreateUser(Guid id, string subject, Guid tenantId) =>
        new()
        {
            Id = id,
            ExternalSubjectId = subject,
            TenantId = tenantId,
            IsActive = true,
            ClaimsVersion = 1
        };

    private static AppUserRole CreateRole(Guid appUserId, string roleName) =>
        new()
        {
            AppUserId = appUserId,
            RoleName = roleName
        };
}
