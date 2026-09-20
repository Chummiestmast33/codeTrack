namespace Backend.Application.Abstractions;

/// <summary>Initial administrator bootstrap (env/UserSecrets only, never the repo).</summary>
public sealed class AdminOptions
{
    public const string SectionName = "Admin";

    public string ControlNumber { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ControlNumber)
        && !string.IsNullOrWhiteSpace(Email)
        && !string.IsNullOrWhiteSpace(Password);
}
