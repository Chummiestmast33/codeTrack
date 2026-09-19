namespace Backend.Application.Abstractions;

/// <summary>Signed URL issued for a direct client-to-storage upload (never proxies bytes).</summary>
public sealed record UploadTicket(string UploadUrl, string StoragePath, DateTimeOffset ExpiresAt);

/// <summary>
/// Object storage contract (Supabase Storage target, RNF-08/RNF-17).
/// Flow: backend validates and issues a short-lived upload URL, the client
/// uploads straight to storage, then confirms with the stored path.
/// The database keeps metadata only (RN-05).
/// </summary>
public interface IFileStorage
{
    Task<UploadTicket> GetUploadUrlAsync(string objectKey, string contentType, long fileSizeBytes, CancellationToken cancellationToken);

    Task<string> GetDownloadUrlAsync(string storagePath, CancellationToken cancellationToken);

    Task DeleteAsync(string storagePath, CancellationToken cancellationToken);
}
