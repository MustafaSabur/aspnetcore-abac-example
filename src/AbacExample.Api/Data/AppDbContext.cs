using AbacExample.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace AbacExample.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<AppUserRole> AppUserRoles => Set<AppUserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(user =>
        {
            user.HasKey(x => x.Id);
            user.HasIndex(x => x.ExternalSubjectId).IsUnique();
            user.Property(x => x.ExternalSubjectId).HasMaxLength(256);
            user.Property(x => x.UserKind).HasConversion<string>();
            user.Property(x => x.Clearance).HasConversion<string>();
        });

        modelBuilder.Entity<AppUserRole>(userRole =>
        {
            userRole.HasKey(x => new { x.AppUserId, x.RoleName });
            userRole.Property(x => x.RoleName).HasMaxLength(128);
            userRole.HasOne(x => x.AppUser)
                .WithMany(x => x.Roles)
                .HasForeignKey(x => x.AppUserId);
        });

        modelBuilder.Entity<Appointment>(appointment =>
        {
            appointment.HasKey(x => x.Id);
            appointment.Property(x => x.Status).HasConversion<string>();
            appointment.Property(x => x.Sensitivity).HasConversion<string>();
        });
    }
}
