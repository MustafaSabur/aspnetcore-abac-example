using AbacExample.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace AbacExample.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(appointment =>
        {
            appointment.HasKey(x => x.Id);
            appointment.Property(x => x.Status).HasConversion<string>();
            appointment.Property(x => x.Sensitivity).HasConversion<string>();
        });
    }
}
