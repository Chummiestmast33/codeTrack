using Backend.Application.Common;
using Backend.Domain.Rules;
using Backend.Tests.Features.Identity;

namespace Backend.Tests.Features.Submissions;

public sealed class FileStorageTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 19, 2, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("main.py", true)]
    [InlineData("Main.PY", true)]
    [InlineData("report.pdf", true)]
    [InlineData("shot.PNG", true)]
    [InlineData("notes.md", true)]
    [InlineData("run.exe", false)]
    [InlineData("lib.dll", false)]
    [InlineData("install.msi", false)]
    [InlineData("script.sh", false)]
    [InlineData("noextension", false)]
    public void Extension_Allow_Block_Lists_Follow_RF16(string fileName, bool allowed)
    {
        if (allowed)
        {
            SubmissionPolicy.EnsureExtension(fileName);
        }
        else
        {
            Assert.Throws<ArgumentException>(() => SubmissionPolicy.EnsureExtension(fileName));
        }
    }

    [Fact]
    public void BuildObjectKey_Strips_Directories_And_Requires_Safe_Name()
    {
        var activity = Guid.NewGuid();
        var user = Guid.NewGuid();

        var key = SubmissionPolicy.BuildObjectKey(activity, user, 2, "../evil.py");

        Assert.StartsWith($"submissions/{activity:N}/{user:N}/2/", key);
        Assert.EndsWith("/evil.py", key);
        Assert.DoesNotContain("..", key);

        Assert.Throws<ArgumentException>(() => SubmissionPolicy.BuildObjectKey(activity, user, 1, "  "));
        Assert.Throws<ArgumentException>(() => SubmissionPolicy.BuildObjectKey(activity, user, 1, "run.exe"));
    }

    [Fact]
    public async Task Signed_Url_Flow_Ticket_Upload_Download_Delete()
    {
        var storage = new FakeFileStorage(new FixedTimeProvider(Now));
        var key = SubmissionPolicy.BuildObjectKey(Guid.NewGuid(), Guid.NewGuid(), 1, "main.py");

        var ticket = await storage.GetUploadUrlAsync(key, "text/x-python", 1024, CancellationToken.None);

        Assert.Equal(key, ticket.StoragePath);
        Assert.Equal(Now.AddMinutes(15), ticket.ExpiresAt);
        Assert.Contains(key, ticket.UploadUrl);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            storage.GetDownloadUrlAsync(key, CancellationToken.None));

        storage.SimulateClientUpload(key);
        var download = await storage.GetDownloadUrlAsync(key, CancellationToken.None);
        Assert.Contains(key, download);

        await storage.DeleteAsync(key, CancellationToken.None);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            storage.GetDownloadUrlAsync(key, CancellationToken.None));
    }

    [Fact]
    public async Task Upload_Ticket_Rejects_Oversized_Files()
    {
        var storage = new FakeFileStorage(new FixedTimeProvider(Now));

        await Assert.ThrowsAsync<ArgumentException>(() => storage.GetUploadUrlAsync(
            "submissions/a/b/1/big.pdf", "application/pdf",
            SubmissionPolicy.MaxFileSizeBytes + 1, CancellationToken.None));
    }
}
