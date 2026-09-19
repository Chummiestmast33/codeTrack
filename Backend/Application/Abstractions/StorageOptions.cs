namespace Backend.Application.Abstractions;

/// <summary>Object storage settings (Supabase Storage target). No real values here.</summary>
public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string Provider { get; set; } = "supabase";

    public string Endpoint { get; set; } = string.Empty;

    public string Bucket { get; set; } = string.Empty;

    public string AccessKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public int UploadUrlExpiryMinutes { get; set; } = 15;

    public int DownloadUrlExpiryMinutes { get; set; } = 60;
}
