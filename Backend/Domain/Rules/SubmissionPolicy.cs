using Backend.Domain.Enums;

namespace Backend.Domain.Rules;

/// <summary>Delivery guards: content per mode (RN-07), 5 MB limit (RN-06),
/// extension allow/block lists (RF-16) and storage key building.</summary>
public static class SubmissionPolicy
{
    public const long MaxFileSizeBytes = 5L * 1024 * 1024;

    public static readonly IReadOnlySet<string> AllowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".cpp", ".c", ".h", ".py", ".java", ".txt", ".md",
        ".png", ".jpg", ".jpeg", ".webp", ".pdf"
    };

    public static readonly IReadOnlySet<string> BlockedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".bat", ".sh", ".out", ".msi"
    };

    public static void EnsureContent(SubmissionMode mode, string? url, string? storagePath)
    {
        var hasUrl = !string.IsNullOrWhiteSpace(url);
        var hasFile = !string.IsNullOrWhiteSpace(storagePath);

        var valid = mode switch
        {
            SubmissionMode.UrlOnly => hasUrl && !hasFile,
            SubmissionMode.FileOnly => hasFile && !hasUrl,
            SubmissionMode.UrlOrFile => hasUrl ^ hasFile,
            SubmissionMode.UrlAndFile => hasUrl && hasFile,
            _ => false
        };

        if (!valid)
        {
            throw new ArgumentException($"Content does not match submission mode '{mode}'.", nameof(mode));
        }
    }

    public static void EnsureFileSize(long fileSizeBytes)
    {
        if (fileSizeBytes > MaxFileSizeBytes)
        {
            throw new ArgumentException($"File exceeds the {MaxFileSizeBytes} bytes limit.", nameof(fileSizeBytes));
        }
    }

    public static void EnsureExtension(string? fileName)
    {
        var extension = Path.GetExtension(fileName ?? string.Empty);
        if (string.IsNullOrEmpty(extension)
            || BlockedExtensions.Contains(extension)
            || !AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException($"File extension '{extension}' is not allowed.", nameof(fileName));
        }
    }

    /// <summary>Unique storage key; strips any client-supplied directories (RNF-17).</summary>
    public static string BuildObjectKey(Guid activityId, Guid userId, int versionNumber, string? fileName)
    {
        if (activityId == Guid.Empty)
        {
            throw new ArgumentException("Activity is required.", nameof(activityId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User is required.", nameof(userId));
        }

        var safeName = Path.GetFileName((fileName ?? string.Empty).Trim());
        if (string.IsNullOrEmpty(safeName))
        {
            throw new ArgumentException("File name is required.", nameof(fileName));
        }

        EnsureExtension(safeName);
        return $"submissions/{activityId:N}/{userId:N}/{versionNumber}/{safeName}";
    }

    public static int NextVersionNumber(int current) => current + 1;
}
