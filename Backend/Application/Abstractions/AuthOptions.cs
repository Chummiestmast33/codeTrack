namespace Backend.Application.Abstractions;

/// <summary>Configurable auth settings. AllowedEmailDomain (D-02) stays
/// unset until the domain is defined; no value is invented here.</summary>
public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public string? AllowedEmailDomain { get; set; }
}
