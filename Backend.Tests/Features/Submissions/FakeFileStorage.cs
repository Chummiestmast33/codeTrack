using Backend.Application.Abstractions;
using Backend.Application.Common;
using Backend.Domain.Rules;
using Backend.Tests.Features.Identity;

namespace Backend.Tests.Features.Submissions;

/// <summary>In-memory IFileStorage double. SimulateClientUpload models the
/// direct client-to-storage step of the signed-URL flow.</summary>
public sealed class FakeFileStorage : IFileStorage
{
    private readonly TimeProvider _time;
    private readonly int _uploadExpiryMinutes;
    private readonly int _downloadExpiryMinutes;
    private readonly HashSet<string> _objects = new(StringComparer.Ordinal);

    public FakeFileStorage(TimeProvider? time = null, int uploadExpiryMinutes = 15, int downloadExpiryMinutes = 60)
    {
        _time = time ?? new FixedTimeProvider(DateTimeOffset.UtcNow);
        _uploadExpiryMinutes = uploadExpiryMinutes;
        _downloadExpiryMinutes = downloadExpiryMinutes;
    }

    public Task<UploadTicket> GetUploadUrlAsync(string objectKey, string contentType, long fileSizeBytes, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        SubmissionPolicy.EnsureFileSize(fileSizeBytes);

        var now = _time.GetUtcNow();
        return Task.FromResult(new UploadTicket(
            $"fake://upload/{objectKey}?content-type={contentType}",
            objectKey,
            now.AddMinutes(_uploadExpiryMinutes)));
    }

    public void SimulateClientUpload(string storagePath) => _objects.Add(storagePath);

    public Task<string> GetDownloadUrlAsync(string storagePath, CancellationToken cancellationToken)
    {
        if (!_objects.Contains(storagePath))
        {
            throw new NotFoundException("Object", storagePath);
        }

        return Task.FromResult($"fake://download/{storagePath}?expires={_time.GetUtcNow().AddMinutes(_downloadExpiryMinutes):O}");
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken)
    {
        _objects.Remove(storagePath);
        return Task.CompletedTask;
    }
}
