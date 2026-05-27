using AbacExample.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AbacExample.Api.Data;

public sealed class AbacExampleDbContext(DbContextOptions<AbacExampleDbContext> options) : DbContext(options)
{
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<AuthorizationUser> AuthorizationUsers => Set<AuthorizationUser>();
    public DbSet<AuthorizationUserRole> AuthorizationUserRoles => Set<AuthorizationUserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuthorizationUser>(user =>
        {
            user.HasKey(x => x.Id);
            user.HasIndex(x => x.ExternalSubjectId).IsUnique();
            user.Property(x => x.ExternalSubjectId).HasMaxLength(256);
        });

        modelBuilder.Entity<AuthorizationUserRole>(userRole =>
        {
            userRole.HasKey(x => new { x.AuthorizationUserId, x.RoleName });
            userRole.Property(x => x.RoleName).HasMaxLength(128);
            userRole.HasOne(x => x.AuthorizationUser)
                .WithMany(x => x.Roles)
                .HasForeignKey(x => x.AuthorizationUserId);
        });

        modelBuilder.Entity<Document>(document =>
        {
            document.HasKey(x => x.Id);
        });
    }
}
