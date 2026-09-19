using System.IdentityModel.Tokens.Jwt;
using Backend.Application.Abstractions;
using Backend.Domain.Entities;
using Backend.Infrastructure;
using Backend.Infrastructure.Persistence;
using Backend.Tests.Features.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Backend.Tests.Infrastructure;

public sealed class InfrastructureWiringTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 19, 2, 0, 0, TimeSpan.Zero);

    private static IConfiguration Config() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=codetrack_dummy",
                ["Jwt:Secret"] = "test-only-secret-min-32-chars!!!!!!",
                ["Jwt:Issuer"] = "codetrack-test",
                ["Jwt:Audience"] = "codetrack-test",
                ["Jwt:ExpiryMinutes"] = "60"
            })
            .Build();

    [Fact]
    public void AddInfrastructure_Resolves_All_Services()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(new FixedTimeProvider(Now));
        services.AddInfrastructure(Config());
        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<TallerDbContext>());
        Assert.IsType<Backend.Infrastructure.Persistence.Repositories.UserRepository>(
            provider.GetRequiredService<IUserRepository>());
        Assert.NotNull(provider.GetService<IUnitOfWork>());
        Assert.NotNull(provider.GetService<IPasswordHasher>());
        Assert.NotNull(provider.GetService<IUserTokenService>());
    }

    [Fact]
    public void PasswordHasher_Roundtrips()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(new FixedTimeProvider(Now));
        services.AddInfrastructure(Config());
        var hasher = services.BuildServiceProvider().GetRequiredService<IPasswordHasher>();

        var hash = hasher.Hash("secret123");
        Assert.NotEqual("secret123", hash);
        Assert.True(hasher.Verify(hash, "secret123"));
        Assert.False(hasher.Verify(hash, "wrongpass"));
    }

    [Fact]
    public void JwtToken_Carries_Identity_Claims_And_Utc_Expiry()
    {
        var tokens = new Backend.Infrastructure.Security.JwtTokenService(
            new OptionsWrapper<Backend.Infrastructure.Security.JwtOptions>(
                new Backend.Infrastructure.Security.JwtOptions
                {
                    Secret = "test-only-secret-min-32-chars!!!!!!",
                    Issuer = "codetrack-test",
                    Audience = "codetrack-test",
                    ExpiryMinutes = 60
                }),
            new FixedTimeProvider(Now));

        var user = User.RegisterStudent("j100", "Ana Paz", "a@x.com", "h", Now);
        var token = tokens.GenerateToken(user);

        Assert.Equal(Now.AddMinutes(60), token.ExpiresAt);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.Token);
        Assert.Equal(user.Id.ToString(), jwt.Subject);
        Assert.Equal("j100", jwt.Claims.First(c => c.Type == "control_number").Value);
        Assert.Equal("codetrack-test", jwt.Issuer);
    }

    [Fact]
    public void AddInfrastructure_Requires_Connection_String_And_Jwt_Secret()
    {
        var noConn = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "test-only-secret-min-32-chars!!!!!!"
            })
            .Build();
        Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().AddInfrastructure(noConn));

        var shortSecret = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost",
                ["Jwt:Secret"] = "short"
            })
            .Build();
        Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().AddInfrastructure(shortSecret));
    }
}
