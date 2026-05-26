using AbacExample.Api.Authorization;
using AbacExample.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AbacExample.Api.Data;

public static class DevelopmentDataSeeder
{
    public const string CaseSupervisorSubject = "case-supervisor-dev";
    public const string CaseAgentSubject = "case-agent-dev";
    public const string AuditorSubject = "auditor-dev";
    public const string OutsideAuditorSubject = "outside-auditor-dev";

    public static readonly Guid TenantAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid TenantBId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static readonly Guid CaseSupervisorUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid CaseAgentUserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid AuditorUserId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    public static readonly Guid OutsideAuditorUserId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    public static readonly Guid PublicCaseFileId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid ConfidentialCaseFileId = Guid.Parse("10000000-0000-0000-0000-000000000002");
    public static readonly Guid AgentOwnedCaseFileId = Guid.Parse("10000000-0000-0000-0000-000000000003");
    public static readonly Guid SupervisorOwnedConfidentialCaseFileId = Guid.Parse("10000000-0000-0000-0000-000000000004");
    public static readonly Guid OutsideTenantCaseFileId = Guid.Parse("20000000-0000-0000-0000-000000000001");

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

        var caseSupervisor = CreateUser(CaseSupervisorUserId, CaseSupervisorSubject, TenantAId);
        var caseAgent = CreateUser(CaseAgentUserId, CaseAgentSubject, TenantAId);
        var auditor = CreateUser(AuditorUserId, AuditorSubject, TenantAId);
        var outsideAuditor = CreateUser(OutsideAuditorUserId, OutsideAuditorSubject, TenantBId);

        db.AppUsers.AddRange(caseSupervisor, caseAgent, auditor, outsideAuditor);
        db.AppUserRoles.AddRange(
            CreateRole(caseSupervisor.Id, AppRoles.CaseSupervisor),
            CreateRole(caseAgent.Id, AppRoles.CaseAgent),
            CreateRole(auditor.Id, AppRoles.Auditor),
            CreateRole(outsideAuditor.Id, AppRoles.Auditor));

        db.CaseFiles.AddRange(
            new CaseFile
            {
                Id = PublicCaseFileId,
                TenantId = TenantAId,
                OwnerId = CaseAgentUserId,
                IsConfidential = false,
                Summary = "Tenant A standard billing dispute owned by case-agent-dev."
            },
            new CaseFile
            {
                Id = ConfidentialCaseFileId,
                TenantId = TenantAId,
                OwnerId = CaseAgentUserId,
                IsConfidential = true,
                Summary = "Tenant A confidential workplace investigation owned by case-agent-dev."
            },
            new CaseFile
            {
                Id = AgentOwnedCaseFileId,
                TenantId = TenantAId,
                OwnerId = CaseAgentUserId,
                IsConfidential = false,
                Summary = "Tenant A customer follow-up assigned to case-agent-dev."
            },
            new CaseFile
            {
                Id = SupervisorOwnedConfidentialCaseFileId,
                TenantId = TenantAId,
                OwnerId = CaseSupervisorUserId,
                IsConfidential = true,
                Summary = "Tenant A supervisor-owned confidential escalation."
            },
            new CaseFile
            {
                Id = OutsideTenantCaseFileId,
                TenantId = TenantBId,
                OwnerId = OutsideAuditorUserId,
                IsConfidential = false,
                Summary = "Tenant B case file visible only to tenant B users."
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
