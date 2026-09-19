using Backend.Api;
using Microsoft.AspNetCore.Cors.Infrastructure;
using CorsOptions = Backend.Api.CorsOptions;using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Tests.Api;

public sealed class CorsPolicyTests
{
    private static (ICorsService Service, ICorsPolicyProvider Provider) ServicesWith(params string[] origins)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = origins.ElementAtOrDefault(0),
                ["Cors:AllowedOrigins:1"] = origins.ElementAtOrDefault(1),
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFrontendCors(config);
        var provider = services.BuildServiceProvider();
        return (
            provider.GetRequiredService<ICorsService>(),
            provider.GetRequiredService<ICorsPolicyProvider>());
    }

    private static DefaultHttpContext ContextWithOrigin(string origin)
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Origin = origin;
        return context;
    }

    [Fact]
    public async Task Listed_Origin_Is_Allowed_With_Any_Header_And_Method()
    {
        var (service, provider) = ServicesWith("http://localhost:5173", "https://app.example.com");
        var context = ContextWithOrigin("http://localhost:5173");
        var policy = await provider.GetPolicyAsync(context, CorsOptions.PolicyName);

        var result = service.EvaluatePolicy(context, policy!);

        Assert.True(result.IsOriginAllowed);
        Assert.True(policy!.AllowAnyHeader);
        Assert.True(policy.AllowAnyMethod);
        Assert.Contains("http://localhost:5173", policy.Origins);
    }

    [Fact]
    public async Task Unlisted_Origin_Is_Rejected()
    {
        var (service, provider) = ServicesWith("http://localhost:5173");
        var context = ContextWithOrigin("http://evil.example.com");
        var policy = await provider.GetPolicyAsync(context, CorsOptions.PolicyName);

        var result = service.EvaluatePolicy(context, policy!);

        Assert.False(result.IsOriginAllowed);
    }
}
