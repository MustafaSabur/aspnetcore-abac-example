using AbacExample.Api.Authorization;
using AbacExample.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AbacExample.Api.Data;

public static class DevelopmentDataSeeder
{
    public const string RecordsManagerSubject = "records-manager-dev";
    public const string DocumentAuthorSubject = "document-author-dev";
    public const string ComplianceAuditorSubject = "compliance-auditor-dev";
    public const string OutsideComplianceAuditorSubject = "outside-compliance-auditor-dev";

    public static readonly Guid TenantAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid TenantBId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static readonly Guid RecordsManagerUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid DocumentAuthorUserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid ComplianceAuditorUserId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    public static readonly Guid OutsideComplianceAuditorUserId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    public static readonly Guid PublicDocumentId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid ConfidentialDocumentId = Guid.Parse("10000000-0000-0000-0000-000000000002");
    public static readonly Guid AuthorOwnedDocumentId = Guid.Parse("10000000-0000-0000-0000-000000000003");
    public static readonly Guid RecordsManagerOwnedConfidentialDocumentId = Guid.Parse("10000000-0000-0000-0000-000000000004");
    public static readonly Guid OutsideTenantDocumentId = Guid.Parse("20000000-0000-0000-0000-000000000001");

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

        var recordsManager = CreateUser(RecordsManagerUserId, RecordsManagerSubject, TenantAId);
        var documentAuthor = CreateUser(DocumentAuthorUserId, DocumentAuthorSubject, TenantAId);
        var auditor = CreateUser(ComplianceAuditorUserId, ComplianceAuditorSubject, TenantAId);
        var outsideAuditor = CreateUser(OutsideComplianceAuditorUserId, OutsideComplianceAuditorSubject, TenantBId);

        db.AppUsers.AddRange(recordsManager, documentAuthor, auditor, outsideAuditor);
        db.AppUserRoles.AddRange(
            CreateRole(recordsManager.Id, AppRoles.RecordsManager),
            CreateRole(documentAuthor.Id, AppRoles.DocumentAuthor),
            CreateRole(auditor.Id, AppRoles.ComplianceAuditor),
            CreateRole(outsideAuditor.Id, AppRoles.ComplianceAuditor));

        db.Documents.AddRange(
            new Document
            {
                Id = PublicDocumentId,
                TenantId = TenantAId,
                OwnerId = DocumentAuthorUserId,
                IsConfidential = false,
                Summary = "Tenant A standard billing dispute owned by document-author-dev."
            },
            new Document
            {
                Id = ConfidentialDocumentId,
                TenantId = TenantAId,
                OwnerId = DocumentAuthorUserId,
                IsConfidential = true,
                Summary = "Tenant A confidential workplace investigation owned by document-author-dev."
            },
            new Document
            {
                Id = AuthorOwnedDocumentId,
                TenantId = TenantAId,
                OwnerId = DocumentAuthorUserId,
                IsConfidential = false,
                Summary = "Tenant A customer follow-up assigned to document-author-dev."
            },
            new Document
            {
                Id = RecordsManagerOwnedConfidentialDocumentId,
                TenantId = TenantAId,
                OwnerId = RecordsManagerUserId,
                IsConfidential = true,
                Summary = "Tenant A records manager-owned confidential escalation."
            },
            new Document
            {
                Id = OutsideTenantDocumentId,
                TenantId = TenantBId,
                OwnerId = OutsideComplianceAuditorUserId,
                IsConfidential = false,
                Summary = "Tenant B document visible only to tenant B users."
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
