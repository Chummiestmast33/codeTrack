using Backend.Domain.Enums;
using Backend.Domain.Rules;

namespace Backend.Domain.Entities;

/// <summary>Versioned student delivery; history is preserved, latest is current (D-03, RF-15 to RF-18).</summary>
public sealed class Submission : AuditableEntity
{
    public Guid ActivityId { get; private set; }

    public Guid UserId { get; private set; }

    public string? Url { get; private set; }

    public string? FileName { get; private set; }

    public string? ContentType { get; private set; }

    public long? FileSizeBytes { get; private set; }

    /// <summary>Path in external storage, never the file bytes (RN-05).</summary>
    public string? StoragePath { get; private set; }

    public string? Comment { get; private set; }

    /// <summary>Instructor's observation on the review (RF-18).</summary>
    public string? InstructorComment { get; private set; }

    public SubmissionStatus Status { get; private set; } = SubmissionStatus.Submitted;

    public int VersionNumber { get; private set; } = 1;

    public DateTimeOffset SubmittedAt { get; private set; }

    public DateTimeOffset? ReviewedAt { get; private set; }

    public Guid? ReviewedBy { get; private set; }

    private Submission()
    {
    }

    private Submission(Guid activityId, Guid userId, string? url, string? storagePath, string? fileName, string? contentType, long? fileSizeBytes, string? comment, int versionNumber, DateTimeOffset submittedAt)
        : base(submittedAt)
    {
        ActivityId = activityId;
        UserId = userId;
        Url = url;
        StoragePath = storagePath;
        FileName = fileName;
        ContentType = contentType;
        FileSizeBytes = fileSizeBytes;
        Comment = comment;
        VersionNumber = versionNumber;
        SubmittedAt = submittedAt;
    }

    public static Submission CreateFirst(Guid activityId, Guid userId, SubmissionMode mode, string? url, string? storagePath, DateTimeOffset submittedAt, string? fileName = null, string? contentType = null, long? fileSizeBytes = null, string? comment = null)
    {
        return Create(activityId, userId, mode, url, storagePath, versionNumber: 1, submittedAt, fileName, contentType, fileSizeBytes, comment);
    }

    public Submission CreateNextVersion(SubmissionMode mode, string? url, string? storagePath, DateTimeOffset submittedAt, string? fileName = null, string? contentType = null, long? fileSizeBytes = null, string? comment = null)
    {
        if (ActivityId == Guid.Empty || UserId == Guid.Empty)
        {
            throw new InvalidOperationException("Cannot version a submission without activity and user.");
        }

        return Create(ActivityId, UserId, mode, url, storagePath, SubmissionPolicy.NextVersionNumber(VersionNumber), submittedAt, fileName, contentType, fileSizeBytes, comment);
    }

    private static Submission Create(Guid activityId, Guid userId, SubmissionMode mode, string? url, string? storagePath, int versionNumber, DateTimeOffset submittedAt, string? fileName, string? contentType, long? fileSizeBytes, string? comment)
    {
        if (activityId == Guid.Empty)
        {
            throw new ArgumentException("Activity is required.", nameof(activityId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User is required.", nameof(userId));
        }

        SubmissionPolicy.EnsureContent(mode, url, storagePath);

        if (!string.IsNullOrWhiteSpace(fileName))
        {
            SubmissionPolicy.EnsureExtension(fileName);
        }

        if (fileSizeBytes.HasValue)
        {
            SubmissionPolicy.EnsureFileSize(fileSizeBytes.Value);
        }

        return new Submission(activityId, userId, url, storagePath, fileName, contentType, fileSizeBytes, comment, versionNumber, submittedAt);
    }

    /// <summary>UTC comparison owned by the domain (RN-09, RF-15).</summary>
    public bool IsLate(DateTimeOffset? dueDate) => dueDate.HasValue && SubmittedAt > dueDate.Value;

    public void Review(Guid reviewerId, SubmissionStatus reviewStatus, DateTimeOffset now, string? instructorComment = null)
    {
        if (reviewerId == Guid.Empty)
        {
            throw new ArgumentException("Reviewer is required.", nameof(reviewerId));
        }

        if (reviewStatus is not SubmissionStatus.Reviewed and not SubmissionStatus.Incomplete)
        {
            throw new ArgumentException("Review must mark the submission as reviewed or incomplete.", nameof(reviewStatus));
        }

        Status = reviewStatus;
        ReviewedBy = reviewerId;
        ReviewedAt = now;
        InstructorComment = instructorComment;
        Touch(now);
    }
}
