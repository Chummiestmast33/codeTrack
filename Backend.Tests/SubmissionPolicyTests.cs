using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Domain.Rules;

namespace Backend.Tests;

public sealed class SubmissionPolicyTests
{
    private static readonly Guid ActivityId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public void Resubmission_Creates_Next_Version_Keeping_First()
    {
        var first = Submission.CreateFirst(ActivityId, UserId, SubmissionMode.UrlOrFile,
            url: "https://github.com/u/r", storagePath: null,
            submittedAt: new DateTimeOffset(2026, 9, 18, 20, 0, 0, TimeSpan.FromHours(-6)));

        var second = first.CreateNextVersion(SubmissionMode.UrlOrFile,
            url: "https://github.com/u/r2", storagePath: null,
            submittedAt: new DateTimeOffset(2026, 9, 18, 21, 0, 0, TimeSpan.FromHours(-6)));

        Assert.Equal(1, first.VersionNumber);
        Assert.Equal(2, second.VersionNumber);
    }

    [Fact]
    public void IsLate_Compares_In_Utc_Across_Offsets()
    {
        // Due 02:00 UTC. Submit 20:30 Mexico (-06:00) = 02:30 UTC -> late.
        // Same wall-clock text in another offset must not confuse the result.
        var due = new DateTimeOffset(2026, 9, 19, 2, 0, 0, TimeSpan.Zero);
        var late = Submission.CreateFirst(ActivityId, UserId, SubmissionMode.UrlOnly,
            url: "https://codeforces.com/x", storagePath: null,
            submittedAt: new DateTimeOffset(2026, 9, 18, 20, 30, 0, TimeSpan.FromHours(-6)));
        var onTime = Submission.CreateFirst(ActivityId, UserId, SubmissionMode.UrlOnly,
            url: "https://codeforces.com/y", storagePath: null,
            submittedAt: new DateTimeOffset(2026, 9, 18, 19, 30, 0, TimeSpan.FromHours(-6)));

        Assert.True(late.IsLate(due));
        Assert.False(onTime.IsLate(due));
        Assert.False(late.IsLate(null));
    }

    [Fact]
    public void Content_Must_Match_Mode()
    {
        Assert.Throws<ArgumentException>(() => Submission.CreateFirst(ActivityId, UserId,
            SubmissionMode.UrlOnly, url: "https://x", storagePath: "f/file.py",
            submittedAt: DateTimeOffset.UtcNow));
        Assert.Throws<ArgumentException>(() => Submission.CreateFirst(ActivityId, UserId,
            SubmissionMode.UrlAndFile, url: "https://x", storagePath: null,
            submittedAt: DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Files_Larger_Than_5MB_Are_Rejected()
    {
        Assert.Throws<ArgumentException>(() => Submission.CreateFirst(ActivityId, UserId,
            SubmissionMode.FileOnly, url: null, storagePath: "f/big.pdf",
            submittedAt: DateTimeOffset.UtcNow, fileSizeBytes: SubmissionPolicy.MaxFileSizeBytes + 1));
    }
}
