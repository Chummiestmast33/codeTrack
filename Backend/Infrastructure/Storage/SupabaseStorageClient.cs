using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Backend.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace Backend.Infrastructure.Storage;

/// <summary>
/// Supabase Storage over plain <see cref="HttpClient"/> (no extra packages).
/// Config mapping: Endpoint = project URL, SecretKey = service_role key
/// (server-side only, never sent to clients), Bucket = private bucket.
/// </summary>
public sealed class SupabaseStorageClient : IFileStorage
{
    private readonly HttpClient _http;
    private readonly StorageOptions _options;
    private readonly TimeProvider _time;

    public SupabaseStorageClient(HttpClient http, IOptions<StorageOptions> options, TimeProvider time)
    {
        _http = http;
        _options = options.Value;
        _time = time;
    }

    public async Task<UploadTicket> GetUploadUrlAsync(string objectKey, string contentType, long fileSizeBytes, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        EnsureConfigured();

        var response = await SendAsync(
            HttpMethod.Post,
            $"storage/v1/object/upload/sign/{_options.Bucket}/{objectKey.TrimStart('/')}",
            null,
            cancellationToken);
        var signed = await ReadAsync<UploadSignResponse>(response, cancellationToken);

        return new UploadTicket(
            CombineUrl(signed.Url),
            objectKey,
            _time.GetUtcNow().AddMinutes(_options.UploadUrlExpiryMinutes));
    }

    public async Task<string> GetDownloadUrlAsync(string storagePath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);
        EnsureConfigured();

        var response = await SendAsync(
            HttpMethod.Post,
            $"storage/v1/object/sign/{_options.Bucket}/{storagePath.TrimStart('/')}",
            new { expiresIn = _options.DownloadUrlExpiryMinutes * 60 },
            cancellationToken);
        var signed = await ReadAsync<DownloadSignResponse>(response, cancellationToken);

        return CombineUrl(signed.SignedURL);
    }

    public async Task DeleteAsync(string storagePath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);
        EnsureConfigured();

        using var response = await SendAsync(
            HttpMethod.Delete,
            $"storage/v1/object/{_options.Bucket}/{storagePath.TrimStart('/')}",
            null,
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.Endpoint)
            || string.IsNullOrWhiteSpace(_options.Bucket)
            || string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            throw new InvalidOperationException(
                "Object storage is not configured (Storage section: Endpoint, Bucket, SecretKey).");
        }
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Add("apikey", _options.SecretKey);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.SecretKey);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        var response = await _http.SendAsync(request, cancellationToken);
        if ((int)response.StatusCode == 401 || (int)response.StatusCode == 403)
        {
            throw new InvalidOperationException(
                $"Object storage rejected the request ({(int)response.StatusCode}). Check the service key and bucket.");
        }

        response.EnsureSuccessStatusCode();
        return response;
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        using (response)
        {
            var payload = await response.Content.ReadFromJsonAsync<T>(cancellationToken);
            return payload ?? throw new InvalidOperationException("Object storage returned an empty response.");
        }
    }

    private string CombineUrl(string pathOrUrl)
    {
        if (Uri.TryCreate(pathOrUrl, UriKind.Absolute, out var absolute))
        {
            return absolute.ToString();
        }

        return $"{_options.Endpoint.TrimEnd('/')}/{pathOrUrl.TrimStart('/')}";
    }

    private sealed record UploadSignResponse(
        [property: JsonPropertyName("url")] string Url,
        [property: JsonPropertyName("token")] string Token);

    private sealed record DownloadSignResponse(
        [property: JsonPropertyName("signedURL")] string SignedURL);
}
