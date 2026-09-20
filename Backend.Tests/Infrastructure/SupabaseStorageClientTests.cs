using System.Net;
using System.Text;
using Backend.Application.Abstractions;
using Backend.Infrastructure.Storage;
using Backend.Tests.Features.Identity;
using Microsoft.Extensions.Options;

namespace Backend.Tests.Infrastructure;

public sealed class SupabaseStorageClientTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 19, 2, 0, 0, TimeSpan.Zero);

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;

        public HttpRequestMessage? LastRequest { get; private set; }

        public string? LastBody { get; private set; }

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
        {
            _respond = respond;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            if (request.Content is not null)
            {
                LastBody = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            return _respond(request);
        }
    }

    private static JsonResponse Json(string json) => new(json);

    private sealed record JsonResponse(string Json)
    {
        public HttpResponseMessage ToMessage(HttpStatusCode status = HttpStatusCode.OK) =>
            new(status)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            };
    }

    private static (SupabaseStorageClient Client, StubHandler Handler) Create(
        Func<HttpRequestMessage, HttpResponseMessage>? respond = null,
        string? secretKey = "service-key")
    {
        var handler = new StubHandler(respond ?? (_ => Json("{\"url\":\"/signed/up?token=t\",\"token\":\"t\",\"signedURL\":\"https://cdn.example.com/signed/dl\"}").ToMessage()));
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://xxxx.supabase.co/") };
        var options = new OptionsWrapper<StorageOptions>(new StorageOptions
        {
            Endpoint = "https://xxxx.supabase.co",
            Bucket = "submissions",
            SecretKey = secretKey ?? string.Empty,
            UploadUrlExpiryMinutes = 15,
            DownloadUrlExpiryMinutes = 60
        });
        return (new SupabaseStorageClient(http, options, new FixedTimeProvider(Now)), handler);
    }

    [Fact]
    public async Task Upload_Signs_Path_And_Returns_Ticket()
    {
        var (client, handler) = Create();

        var ticket = await client.GetUploadUrlAsync("submissions/a/b/1/main.py", "text/x-python", 1024, CancellationToken.None);

        Assert.Equal("submissions/a/b/1/main.py", ticket.StoragePath);
        Assert.Equal(Now.AddMinutes(15), ticket.ExpiresAt);
        Assert.StartsWith("https://xxxx.supabase.co/signed/up", ticket.UploadUrl);
        Assert.Contains("/storage/v1/object/upload/sign/submissions/submissions/a/b/1/main.py", handler.LastRequest!.RequestUri!.ToString());
        Assert.True(handler.LastRequest.Headers.Contains("apikey"));
        Assert.NotNull(handler.LastRequest.Headers.Authorization);
    }

    [Fact]
    public async Task Download_Uses_ExpiresIn_And_Returns_Absolute_Url()
    {
        var (client, handler) = Create();

        var url = await client.GetDownloadUrlAsync("submissions/a/b/1/main.py", CancellationToken.None);

        Assert.Equal("https://cdn.example.com/signed/dl", url);
        Assert.Contains("\"expiresIn\":3600", handler.LastBody);
    }

    [Fact]
    public async Task Absolute_Url_With_Query_Is_Returned_Untouched()
    {
        var (client, _) = Create(_ => Json("{\"url\":\"https://xxxx.supabase.co/signed/up?token=t\",\"token\":\"t\",\"signedURL\":\"https://cdn.example.com/signed/dl\"}").ToMessage());

        var ticket = await client.GetUploadUrlAsync("k", "text/plain", 1, CancellationToken.None);

        Assert.Equal("https://xxxx.supabase.co/signed/up?token=t", ticket.UploadUrl);
    }

    [Fact]
    public async Task Delete_Calls_Object_Endpoint()
    {
        var (client, handler) = Create(_ => new HttpResponseMessage(HttpStatusCode.OK));

        await client.DeleteAsync("submissions/a/b/1/main.py", CancellationToken.None);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest!.Method);
        Assert.Contains("/storage/v1/object/submissions/submissions/a/b/1/main.py", handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Unauthorized_Does_Not_Leak_Key()
    {
        var (client, _) = Create(
            _ => new HttpResponseMessage(HttpStatusCode.Unauthorized),
            secretKey: "super-secret-key");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.GetDownloadUrlAsync("k", CancellationToken.None));
        Assert.DoesNotContain("super-secret-key", ex.Message);
    }

    [Fact]
    public async Task Missing_Config_Throws_Clear_Error()
    {
        var (client, _) = Create(secretKey: "");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.GetUploadUrlAsync("k", "text/plain", 1, CancellationToken.None));
        Assert.Contains("SecretKey", ex.Message);
    }
}
