using Backend.Application.Abstractions;
using Backend.Domain.Entities;
using Backend.Infrastructure.Health;
using Backend.Infrastructure.Persistence;
using Backend.Tests.Features.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Backend.Tests.Infrastructure;

public sealed class OperationalTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 19, 2, 0, 0, TimeSpan.Zero);

    private sealed class ActorContext(Guid? actor) : IUserContext
    {
        public Guid? CurrentUserId => actor;
    }

    private static TallerDbContext InMemoryDb(IUserContext users, string name) =>
        new(new DbContextOptionsBuilder<TallerDbContext>()
            .UseInMemoryDatabase(name)
            .Options,
            users);

    [Fact]
    public async Task SaveChanges_Stamps_Actor_And_Utc_Timestamps()
    {
        var name = Guid.NewGuid().ToString();
        var actor = Guid.NewGuid();
        var other = Guid.NewGuid();
        var before = Now.AddMinutes(-5);

        Topic topic;
        using (var db = InMemoryDb(new ActorContext(actor), name))
        {
            topic = Topic.Create("T", null, 1, before);
            db.Topics.Add(topic);
            await db.SaveChangesAsync();
        }

        Assert.Equal(actor, topic.CreatedBy);
        Assert.Equal(actor, topic.UpdatedBy);
        Assert.True(topic.CreatedAt >= before);

        using (var db = InMemoryDb(new ActorContext(other), name))
        {
            db.Topics.Update(topic);
            await db.SaveChangesAsync();
        }

        Assert.Equal(actor, topic.CreatedBy);
        Assert.Equal(other, topic.UpdatedBy);
    }

    [Fact]
    public async Task Health_Is_Unhealthy_When_Postgres_Unreachable()
    {
        using var db = new TallerDbContext(
            new DbContextOptionsBuilder<TallerDbContext>()
                .UseNpgsql("Host=127.0.0.1;Port=1;Database=x;Username=x;Password=x;Timeout=2")
                .Options,
            new ActorContext(null));

        var check = new PostgresHealthCheck(db);
        var result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    private static ServiceProvider SeedProvider(string dbName, AdminOptions admin)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<TimeProvider>(new FixedTimeProvider(Now));
        services.AddSingleton<IUserContext>(new ActorContext(null));
        services.AddSingleton<IPasswordHasher, FakePasswordHasher>();
        services.AddSingleton(Options.Create(admin));
        services.AddDbContext<TallerDbContext>(o => o.UseInMemoryDatabase(dbName));
        services.AddScoped<AdminSeedHostedService>();
        return services.BuildServiceProvider();
    }

    private static AdminOptions ConfiguredAdmin() => new()
    {
        ControlNumber = " root1 ",
        FullName = "Root",
        Email = "root@x.com",
        Password = "secret123"
    };

    [Fact]
    public async Task AdminSeed_Creates_Approved_Admin_Once()
    {
        var dbName = Guid.NewGuid().ToString();
        using var provider = SeedProvider(dbName, ConfiguredAdmin());
        using var scope = provider.CreateScope();
        var seed = scope.ServiceProvider.GetRequiredService<AdminSeedHostedService>();

        await seed.StartAsync(CancellationToken.None);
        await seed.StartAsync(CancellationToken.None);

        using var verify = provider.CreateScope();
        var db = verify.ServiceProvider.GetRequiredService<TallerDbContext>();
        var single = Assert.Single(await db.Users.ToListAsync());
        Assert.Equal("root1", single.ControlNumber);
        Assert.Equal(Domain.Enums.UserRole.Administrator, single.Role);
        Assert.True(single.CanSignIn);
    }

    [Fact]
    public async Task AdminSeed_Skips_When_Unconfigured()
    {
        var dbName = Guid.NewGuid().ToString();
        using var provider = SeedProvider(dbName, new AdminOptions());
        using var scope = provider.CreateScope();

        await scope.ServiceProvider.GetRequiredService<AdminSeedHostedService>()
            .StartAsync(CancellationToken.None);

        using var verify = provider.CreateScope();
        var db = verify.ServiceProvider.GetRequiredService<TallerDbContext>();
        Assert.Empty(await db.Users.ToListAsync());
    }
}
