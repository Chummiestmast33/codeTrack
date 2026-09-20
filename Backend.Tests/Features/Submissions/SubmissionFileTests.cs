using Backend.Application.Abstractions;
using Backend.Application.Common;
using Backend.Application.Features.Submissions;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Moq;

namespace Backend.Tests.Features.Submissions;

public sealed class SubmissionFileTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 20, 0, 0, 0, TimeSpan.Zero);
    private readonly Mock<ISubmissionRepository> _submissions = new(MockBehavior.Strict);
    private readonly Mock<IUserRepository> _users = new(MockBehavior.Strict);
    private readonly Mock<IFileStorage> _storage = new(MockBehavior.Strict);

    private GetSubmissionFileHandler Handler() => new(_submissions.Object, _users.Object, _storage.Object);

    private static User Admin()
    {
        var user = User.RegisterAdministrator("admin-test", "Test Admin", "admin@example.invalid", "test-hash", Now);
        user.Approve(Now);
        return user;
    }

    [Fact]
    public async Task SignsTheRequestedHistoricalVersionAndPropagatesCancellation()
    {
        var admin = Admin();
        var first = Submission.CreateFirst(Guid.NewGuid(), Guid.NewGuid(), SubmissionMode.FileOnly,
            null, "submissions/test/1/main.txt", Now, "main.txt", "text/plain", 10);
        var latest = first.CreateNextVersion(SubmissionMode.FileOnly, null,
            "submissions/test/2/main.txt", Now.AddMinutes(1), "main.txt", "text/plain", 12);
        using var cancellation = new CancellationTokenSource();
        var ct = cancellation.Token;
        _users.Setup(x => x.GetByIdAsync(admin.Id, ct)).ReturnsAsync(admin);
        _submissions.Setup(x => x.GetByIdAsync(first.Id, ct)).ReturnsAsync(first);
        _storage.Setup(x => x.GetDownloadUrlAsync(first.StoragePath!, ct)).ReturnsAsync("https://storage.example.invalid/signed");

        var result = await Handler().Handle(new(first.Id, admin.Id), ct);

        Assert.Equal("main.txt", result.FileName);
        Assert.Equal("https://storage.example.invalid/signed", result.DownloadUrl);
        Assert.NotEqual(first.StoragePath, latest.StoragePath);
        _storage.VerifyAll();
    }

    [Theory]
    [InlineData("student")]
    [InlineData("inactive")]
    [InlineData("pending")]
    [InlineData("missing")]
    public async Task RejectsUnauthorizedActorsBeforeLookingUpFiles(string actor)
    {
        var user = actor == "student"
            ? User.RegisterStudent("student-test", "Test Student", "student@example.invalid", "test-hash", Now)
            : Admin();
        if (actor == "student") user.Approve(Now);
        if (actor == "inactive") user.Deactivate(Now);
        if (actor == "pending") user = User.RegisterAdministrator("pending-test", "Test Admin", "admin@example.invalid", "test-hash", Now);
        _users.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(actor == "missing" ? null : user);

        await Assert.ThrowsAsync<ForbiddenException>(() => Handler().Handle(new(Guid.NewGuid(), user.Id), default));
        _submissions.VerifyNoOtherCalls();
        _storage.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task MissingSubmissionOrUrlOnlySubmissionDoesNotIssueSignedUrl(bool missing)
    {
        var admin = Admin();
        var submission = Submission.CreateFirst(Guid.NewGuid(), Guid.NewGuid(), SubmissionMode.UrlOnly,
            "https://example.org/work", null, Now);
        _users.Setup(x => x.GetByIdAsync(admin.Id, It.IsAny<CancellationToken>())).ReturnsAsync(admin);
        _submissions.Setup(x => x.GetByIdAsync(submission.Id, It.IsAny<CancellationToken>())).ReturnsAsync(missing ? null : submission);

        await Assert.ThrowsAsync<NotFoundException>(() => Handler().Handle(new(submission.Id, admin.Id), default));
        _storage.VerifyNoOtherCalls();
    }
}
