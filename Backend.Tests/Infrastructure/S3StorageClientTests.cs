using Amazon.S3;
using Amazon.S3.Model;
using Backend.Application.Abstractions;
using Backend.Infrastructure.Storage;
using Backend.Tests.Features.Identity;
using Microsoft.Extensions.Options;
using Moq;

namespace Backend.Tests.Infrastructure;

public sealed class S3StorageClientTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 19, 2, 0, 0, TimeSpan.Zero);

    private static (S3StorageClient Client, Mock<IAmazonS3> S3) Create()
    {
        var s3 = new Mock<IAmazonS3>(MockBehavior.Strict);
        var options = new OptionsWrapper<StorageOptions>(new StorageOptions
        {
            Endpoint = "https://xxx.storage.supabase.co/storage/v1/s3",
            Region = "us-west-2",
            Bucket = "submissions",
            AccessKey = "access-key",
            SecretKey = "secret-key",
            UploadUrlExpiryMinutes = 15,
            DownloadUrlExpiryMinutes = 60
        });
        return (new S3StorageClient(s3.Object, options, new FixedTimeProvider(Now)), s3);
    }

    [Fact]
    public async Task Upload_Signs_Put_With_Content_Type_And_Expiry()
    {
        var (client, s3) = Create();
        GetPreSignedUrlRequest? actual = null;
        s3.Setup(x => x.GetPreSignedURLAsync(It.IsAny<GetPreSignedUrlRequest>()))
            .Callback<GetPreSignedUrlRequest>(r => actual = r)
            .ReturnsAsync("https://signed.example.com/up?sig=abc");

        var ticket = await client.GetUploadUrlAsync("submissions/a/b/1/main.py", "text/x-python", 1024, CancellationToken.None);

        Assert.Equal("https://signed.example.com/up?sig=abc", ticket.UploadUrl);
        Assert.Equal("submissions/a/b/1/main.py", ticket.StoragePath);
        Assert.Equal(Now.AddMinutes(15), ticket.ExpiresAt);
        Assert.NotNull(actual);
        Assert.Equal(HttpVerb.PUT, actual!.Verb);
        Assert.Equal("submissions", actual.BucketName);
        Assert.Equal("submissions/a/b/1/main.py", actual.Key);
        Assert.Equal("text/x-python", actual.ContentType);
        Assert.True(actual.Expires > Now.UtcDateTime);
    }

    [Fact]
    public async Task Download_Signs_Get_With_Expiry()
    {
        var (client, s3) = Create();
        GetPreSignedUrlRequest? actual = null;
        s3.Setup(x => x.GetPreSignedURLAsync(It.IsAny<GetPreSignedUrlRequest>()))
            .Callback<GetPreSignedUrlRequest>(r => actual = r)
            .ReturnsAsync("https://signed.example.com/dl?sig=xyz");

        var url = await client.GetDownloadUrlAsync("submissions/a/b/1/main.py", CancellationToken.None);

        Assert.Equal("https://signed.example.com/dl?sig=xyz", url);
        Assert.NotNull(actual);
        Assert.Equal(HttpVerb.GET, actual!.Verb);
        Assert.Equal("submissions", actual.BucketName);
    }

    [Fact]
    public async Task Delete_Removes_Object()
    {
        var (client, s3) = Create();
        s3.Setup(x => x.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteObjectResponse());

        await client.DeleteAsync("submissions/a/b/1/main.py", CancellationToken.None);

        s3.Verify(x => x.DeleteObjectAsync(
            It.Is<DeleteObjectRequest>(r => r.BucketName == "submissions" && r.Key == "submissions/a/b/1/main.py"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Missing_Config_Throws_Clear_Error()
    {
        var s3 = new Mock<IAmazonS3>(MockBehavior.Strict);
        var options = new OptionsWrapper<StorageOptions>(new StorageOptions());
        var client = new S3StorageClient(s3.Object, options, new FixedTimeProvider(Now));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.GetUploadUrlAsync("k", "text/plain", 1, CancellationToken.None));
        Assert.Contains("SecretKey", ex.Message);
    }
}
