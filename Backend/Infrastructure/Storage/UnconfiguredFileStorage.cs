using Backend.Application.Abstractions;

namespace Backend.Infrastructure.Storage;

/// <summary>Placeholder until the real object-storage client lands.
/// Boot-safe: resolves in DI, but fails with a clear message when used.</summary>
public sealed class UnconfiguredFileStorage : IFileStorage
{
    private static InvalidOperationException Missing() => new(
        "Object storage is not configured. Register a real IFileStorage implementation (Supabase Storage target).");

    public Task<UploadTicket> GetUploadUrlAsync(string objectKey, string contentType, long fileSizeBytes, CancellationToken cancellationToken) =>
        throw Missing();

    public Task<string> GetDownloadUrlAsync(string storagePath, CancellationToken cancellationToken) =>
        throw Missing();

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken) =>
        throw Missing();
}
