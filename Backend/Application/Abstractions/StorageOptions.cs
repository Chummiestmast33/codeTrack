namespace Backend.Application.Abstractions;

/// <summary>Object storage settings (S3-compatible: Supabase Storage, R2, MinIO).
/// Endpoint = S3 endpoint URL, AccessKey/SecretKey = S3 credentials, all server-side only.</summary>
public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string Provider { get; set; } = "s3";

    public string Endpoint { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string Bucket { get; set; } = string.Empty;

    public string AccessKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public int UploadUrlExpiryMinutes { get; set; } = 15;

    public int DownloadUrlExpiryMinutes { get; set; } = 60;
}
