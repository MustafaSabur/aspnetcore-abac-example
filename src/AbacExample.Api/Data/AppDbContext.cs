using AbacExample.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AbacExample.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<AppUserRole> AppUserRoles => Set<AppUserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(user =>
        {
            user.HasKey(x => x.Id);
            user.HasIndex(x => x.ExternalSubjectId).IsUnique();
            user.Property(x => x.ExternalSubjectId).HasMaxLength(256);
        });

        modelBuilder.Entity<AppUserRole>(userRole =>
        {
            userRole.HasKey(x => new { x.AppUserId, x.RoleName });
            userRole.Property(x => x.RoleName).HasMaxLength(128);
            userRole.HasOne(x => x.AppUser)
                .WithMany(x => x.Roles)
                .HasForeignKey(x => x.AppUserId);
        });

        modelBuilder.Entity<Document>(document =>
        {
            document.HasKey(x => x.Id);
        });
    }
}
