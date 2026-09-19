namespace Backend.Infrastructure.Security;

/// <summary>JWT settings from environment (RNF-14). Secret must be ≥32 chars.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 60;
}
