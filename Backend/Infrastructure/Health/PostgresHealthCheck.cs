using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Backend.Infrastructure.Health;

/// <summary>Liveness plus Postgres connectivity. The applied-migrations
/// count is best-effort diagnostics: it must never fail the check,
/// because a slow catalog query is not an outage.</summary>
public sealed class PostgresHealthCheck : IHealthCheck
{
    private static readonly TimeSpan DiagnosticsTimeout = TimeSpan.FromSeconds(5);

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

            return HealthCheckResult.Healthy(data: new Dictionary<string, object>
            {
                ["appliedMigrations"] = await AppliedMigrationsOrUnknownAsync(cancellationToken)
            });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Postgres check failed.", ex);
        }
    }

    private async Task<string> AppliedMigrationsOrUnknownAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var timeout = new CancellationTokenSource(DiagnosticsTimeout);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
            var applied = await _db.Database.GetAppliedMigrationsAsync(linked.Token);
            return applied.Count().ToString();
        }
        catch
        {
            return "unknown";
        }
    }
}
