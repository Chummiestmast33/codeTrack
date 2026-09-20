using Backend.Application.Abstractions;
using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence;

/// <summary>EF Core context. DateTimeOffset maps to timestamptz (RN-09).</summary>
public sealed class TallerDbContext : DbContext
{
    private readonly IUserContext _users;

    public TallerDbContext(DbContextOptions<TallerDbContext> options, IUserContext users)
        : base(options)
    {
        _users = users;
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
        var actor = _users.CurrentUserId;
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(e => e.CreatedAt).CurrentValue = now;
                entry.Property(e => e.UpdatedAt).CurrentValue = now;
                entry.Property(e => e.CreatedBy).CurrentValue = actor;
                entry.Property(e => e.UpdatedBy).CurrentValue = actor;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(e => e.UpdatedAt).CurrentValue = now;
                entry.Property(e => e.UpdatedBy).CurrentValue = actor;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
