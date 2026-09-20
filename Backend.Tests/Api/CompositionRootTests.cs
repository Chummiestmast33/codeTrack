using Backend.Application;
using Backend.Application.Abstractions;
using Backend.Infrastructure;
using Backend.Tests.Features.Identity;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Tests.Api;

/// <summary>Guards the composition root: every MediatR handler must resolve.</summary>
public sealed class CompositionRootTests
{
    private static ServiceProvider BuildProvider()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=codetrack_dummy",
                ["Jwt:Secret"] = "test-only-secret-min-32-chars!!!!!!",
                ["Jwt:Issuer"] = "codetrack-test",
                ["Jwt:Audience"] = "codetrack-test"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<TimeProvider>(new FixedTimeProvider(DateTimeOffset.UtcNow));
        services.AddApplication();
        services.AddInfrastructure(config);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void All_Handlers_Resolve()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var handlerTypes = typeof(DependencyInjection).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .Select(i => new { Service = i, Implementation = t }))
            .ToList();

        Assert.NotEmpty(handlerTypes);
        foreach (var handler in handlerTypes)
        {
            var resolved = scope.ServiceProvider.GetService(handler.Service);
            Assert.NotNull(resolved);
        }
    }

    [Fact]
    public async Task Unconfigured_Storage_Fails_With_Clear_Message()
    {
        using var provider = BuildProvider();
        var storage = provider.GetRequiredService<IFileStorage>();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            storage.GetUploadUrlAsync("k", "text/plain", 1, CancellationToken.None));
        Assert.Contains("not configured", ex.Message);
    }
}
