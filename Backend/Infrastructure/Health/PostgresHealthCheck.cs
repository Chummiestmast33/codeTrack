using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Backend.Infrastructure.Health;

/// <summary>Liveness plus Postgres connectivity and applied-migrations count.</summary>
public sealed class PostgresHealthCheck : IHealthCheck
{
    private readonly Persistence.TallerDbContext _db;

    public PostgresHealthCheck(Persistence.TallerDbContext db)
    {
        _db = db;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _db.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("Postgres is unreachable.");
            }

            var applied = await _db.Database.GetAppliedMigrationsAsync(cancellationToken);
            return HealthCheckResult.Healthy(data: new Dictionary<string, object>
            {
                ["appliedMigrations"] = applied.Count()
            });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Postgres check failed.", ex);
        }
    }
}
