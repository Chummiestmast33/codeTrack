using System.Text;
using Backend.Application.Abstractions;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Persistence.Repositories;
using Backend.Infrastructure.Qr;
using Backend.Infrastructure.Health;
using Backend.Infrastructure.Reporting;
using Backend.Infrastructure.Security;
using Backend.Infrastructure.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

        services.AddDbContext<TallerDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITopicRepository, TopicRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<IQrTokenRepository, QrTokenRepository>();
        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();
        services.AddScoped<IProgressRepository, ProgressRepository>();
        services.AddScoped<IReportExporter, ReportExporter>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IPasswordHasher, AspNetPasswordHasher>();
        services.AddSingleton<IUserTokenService, JwtTokenService>();
        services.AddSingleton<IQrCodeGenerator, QrCodeGenerator>();
        services.AddSingleton<IFileStorage, UnconfiguredFileStorage>();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<QrOptions>(configuration.GetSection(QrOptions.SectionName));
        services.Configure<ReportOptions>(configuration.GetSection(ReportOptions.SectionName));
        services.Configure<AdminOptions>(configuration.GetSection(AdminOptions.SectionName));
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, HttpUserContext>();
        services.AddHealthChecks().AddCheck<PostgresHealthCheck>("postgres");
        services.AddHostedService<AdminSeedHostedService>();

        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        if (jwt.Secret.Length < 32)
        {
            throw new InvalidOperationException("Jwt Secret must be at least 32 characters.");
        }

        services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                    ClockSkew = TimeSpan.FromMinutes(2)
                };
            });

        return services;
    }

    public static IApplicationBuilder MapAppHealthChecks(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var postgres = report.Entries.TryGetValue("postgres", out var entry)
                    ? entry.Data.TryGetValue("appliedMigrations", out var count) ? count?.ToString() : null
                    : null;
                await context.Response.WriteAsJsonAsync(new
                {
                    status = report.Status.ToString(),
                    postgres = postgres is null ? "unknown" : "up",
                    appliedMigrations = postgres ?? "0"
                });
            }
        });

        return app;
    }
}
