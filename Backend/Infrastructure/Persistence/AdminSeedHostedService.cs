using Backend.Application.Abstractions;
using Backend.Domain.Entities;
using Backend.Domain.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Backend.Infrastructure.Persistence;

/// <summary>Creates the initial administrator once (idempotent, no secrets logged).</summary>
public sealed class AdminSeedHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly AdminOptions _options;
    private readonly TimeProvider _time;
    private readonly ILogger<AdminSeedHostedService> _logger;

    public AdminSeedHostedService(
        IServiceScopeFactory scopes,
        IOptions<AdminOptions> options,
        TimeProvider time,
        ILogger<AdminSeedHostedService> logger)
    {
        _scopes = scopes;
        _options = options.Value;
        _time = time;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var admin = _options;
        if (!admin.IsConfigured)
        {
            _logger.LogInformation("Admin seed skipped: Admin section is not configured.");
            return;
        }

        try
        {
            await SeedAsync(admin, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Never take down the host: the admin can be seeded on the next healthy boot.
            _logger.LogError(ex, "Admin seed failed; startup continues.");
        }
    }

    private async Task SeedAsync(AdminOptions admin, CancellationToken cancellationToken)
    {
        using var scope = _scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TallerDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var controlNumber = ControlNumberRules.Normalize(admin.ControlNumber);
        if (await db.Users.AnyAsync(u => u.ControlNumber == controlNumber, cancellationToken))
        {
            _logger.LogInformation("Admin seed skipped: {ControlNumber} already exists.", controlNumber);
            return;
        }

        var user = User.RegisterAdministrator(
            controlNumber, admin.FullName, admin.Email, hasher.Hash(admin.Password), _time.GetUtcNow());
        user.Approve(_time.GetUtcNow());
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Admin seed created: {ControlNumber}.", controlNumber);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
