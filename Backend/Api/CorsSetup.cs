using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Api;

public static class CorsSetup
{
    public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CorsOptions>(configuration.GetSection(CorsOptions.SectionName));
        services.AddCors(options =>
        {
            options.AddPolicy(CorsOptions.PolicyName, policy =>
            {
                var origins = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()?.AllowedOrigins
                    ?.Where(o => !string.IsNullOrWhiteSpace(o)).ToArray()
                    ?? [];
                // Closed origin list on purpose: Bearer tokens travel in headers (no cookies),
                // so AllowCredentials is unnecessary and AllowAnyOrigin would be insecure.
                policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
            });
        });

        return services;
    }
}
