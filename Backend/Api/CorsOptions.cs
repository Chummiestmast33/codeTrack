namespace Backend.Api;

/// <summary>Frontend origins allowed to call the API (browser CORS).
/// Production origins come from environment (Cors__AllowedOrigins__0, ...).</summary>
public sealed class CorsOptions
{
    public const string SectionName = "Cors";

    public const string PolicyName = "Frontend";

    public string[] AllowedOrigins { get; set; } = [];
}
