using event_tsika_back.Domain;
using Microsoft.EntityFrameworkCore;

namespace event_tsika_back.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    // SQLite perd le Kind des dates : on force UTC à la lecture pour que l'API sérialise "...Z".
    protected override void ConfigureConventions(ModelConfigurationBuilder c) =>
        c.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();

    private sealed class UtcDateTimeConverter() : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
        v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(u => u.Email).IsUnique();
        b.Entity<User>().Property(u => u.Role).HasConversion<string>();
        b.Entity<Event>().Property(e => e.Category).HasConversion<string>();
        b.Entity<Event>().HasIndex(e => e.StartsAtUtc);
        b.Entity<Event>().HasOne(e => e.Organizer).WithMany().HasForeignKey(e => e.OrganizerId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Reservation>().Property(r => r.Status).HasConversion<string>();
        b.Entity<Reservation>().HasIndex(r => r.TicketCode).IsUnique();
        b.Entity<Reservation>().HasOne(r => r.Event).WithMany().HasForeignKey(r => r.EventId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Reservation>().HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
