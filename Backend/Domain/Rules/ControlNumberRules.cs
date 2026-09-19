namespace Backend.Domain.Rules;

/// <summary>
/// Control number handling (RN-01, RN-02 canceled):
/// required, unique (uniqueness enforced in PostgreSQL), trimmed,
/// stored as text. No institutional format, length, prefix or digits rule.
/// </summary>
public static class ControlNumberRules
{
    public static string Normalize(string? controlNumber)
    {
        if (string.IsNullOrWhiteSpace(controlNumber))
        {
            throw new ArgumentException("Control number is required.", nameof(controlNumber));
        }

        return controlNumber.Trim();
    }
}
