namespace Backend.Application.Abstractions;

/// <summary>
/// Official report header (RN-08). Values are deployment configuration,
/// never hardcoded personal data: they come from the Reports section
/// (UserSecrets in dev, environment in production).
/// </summary>
public sealed class ReportOptions
{
    public const string SectionName = "Reports";

    public string ProjectName { get; set; } = string.Empty;

    public string Period { get; set; } = string.Empty;

    public string Responsible { get; set; } = string.Empty;

    public string Advisor { get; set; } = string.Empty;

    public void EnsureConfigured()
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(ProjectName))
        {
            missing.Add(nameof(ProjectName));
        }

        if (string.IsNullOrWhiteSpace(Period))
        {
            missing.Add(nameof(Period));
        }

        if (string.IsNullOrWhiteSpace(Responsible))
        {
            missing.Add(nameof(Responsible));
        }

        if (string.IsNullOrWhiteSpace(Advisor))
        {
            missing.Add(nameof(Advisor));
        }

        if (missing.Count > 0)
        {
            throw new Common.ReportsNotConfiguredException(
                $"Report header is not configured (Reports section): {string.Join(", ", missing)}.");
        }
    }
}
