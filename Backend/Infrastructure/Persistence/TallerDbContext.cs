using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence;

/// <summary>EF Core context. DateTimeOffset maps to timestamptz (RN-09).</summary>
public sealed class TallerDbContext : DbContext
{
    public TallerDbContext(DbContextOptions<TallerDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Topic> Topics => Set<Topic>();

    public DbSet<Session> Sessions => Set<Session>();

    public DbSet<SessionTopic> SessionTopics => Set<SessionTopic>();

    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();

    public DbSet<QrToken> QrTokens => Set<QrToken>();

    public DbSet<Activity> Activities => Set<Activity>();

    public DbSet<Submission> Submissions => Set<Submission>();

    public DbSet<ProgressRecord> ProgressRecords => Set<ProgressRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TallerDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(e => e.CreatedAt).CurrentValue = now;
                entry.Property(e => e.UpdatedAt).CurrentValue = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(e => e.UpdatedAt).CurrentValue = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
