using Amazon.S3;
using Amazon.S3.Model;
using Backend.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace Backend.Infrastructure.Storage;

/// <summary>
/// S3-compatible object storage (Supabase Storage, R2, MinIO): signed URLs
/// for direct client upload/download, metadata only in the database (RN-05).
/// Credentials stay server-side; clients only see short-lived URLs.
/// </summary>
public sealed class S3StorageClient : IFileStorage
{
    private readonly IAmazonS3 _s3;
    private readonly StorageOptions _options;
    private readonly TimeProvider _time;

    public S3StorageClient(IAmazonS3 s3, IOptions<StorageOptions> options, TimeProvider time)
    {
        _s3 = s3;
        _options = options.Value;
        _time = time;
    }

    public static AmazonS3Client CreateClient(StorageOptions options) => new(
        options.AccessKey,
        options.SecretKey,
        new AmazonS3Config
        {
            // No trailing slash: the SDK appends one, and a double slash
            // breaks signatures once the server normalizes the path.
            ServiceURL = options.Endpoint.TrimEnd('/'),
            AuthenticationRegion = options.Region,
            ForcePathStyle = true
        });

    public async Task<UploadTicket> GetUploadUrlAsync(string objectKey, string contentType, long fileSizeBytes, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        EnsureConfigured();

        var expiresAt = _time.GetUtcNow().AddMinutes(_options.UploadUrlExpiryMinutes);
        var url = await _s3.GetPreSignedURLAsync(new GetPreSignedUrlRequest
        {
            BucketName = _options.Bucket,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            Expires = expiresAt.UtcDateTime,
            ContentType = contentType
        });

        return new UploadTicket(url, objectKey, expiresAt);
    }

    public async Task<string> GetDownloadUrlAsync(string storagePath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);
        EnsureConfigured();

        return await _s3.GetPreSignedURLAsync(new GetPreSignedUrlRequest
        {
            BucketName = _options.Bucket,
            Key = storagePath,
            Verb = HttpVerb.GET,
            Expires = _time.GetUtcNow().AddMinutes(_options.DownloadUrlExpiryMinutes).UtcDateTime
        });
    }

    public async Task DeleteAsync(string storagePath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);
        EnsureConfigured();

        try
        {
            await _s3.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _options.Bucket,
                Key = storagePath
            }, cancellationToken);
        }
        catch (AmazonS3Exception ex)
        {
            throw new InvalidOperationException(
                $"Object storage delete failed ({ex.StatusCode}). Check bucket and credentials.");
        }
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.Endpoint)
            || string.IsNullOrWhiteSpace(_options.Region)
            || string.IsNullOrWhiteSpace(_options.Bucket)
            || string.IsNullOrWhiteSpace(_options.AccessKey)
            || string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            throw new InvalidOperationException(
                "Object storage is not configured (Storage section: Endpoint, Region, Bucket, AccessKey, SecretKey).");
        }
    }
}
