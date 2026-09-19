using Backend.Domain.Enums;

namespace Backend.Domain.Entities;

/// <summary>Instructor activity with Markdown content (RF-12 to RF-14).</summary>
public sealed class Activity : AuditableEntity
{
    public string Title { get; private set; } = string.Empty;

    public string MarkdownContent { get; private set; } = string.Empty;

    public Guid TopicId { get; private set; }

    public Guid? SessionId { get; private set; }

    public DateTimeOffset? DueDate { get; private set; }

    public ActivityStatus Status { get; private set; } = ActivityStatus.Draft;

    public SubmissionMode SubmissionMode { get; private set; }

    private Activity()
    {
    }

    private Activity(string title, string markdownContent, Guid topicId, Guid? sessionId, DateTimeOffset? dueDate, SubmissionMode submissionMode, DateTimeOffset now)
        : base(now)
    {
        Title = title;
        MarkdownContent = markdownContent;
        TopicId = topicId;
        SessionId = sessionId;
        DueDate = dueDate;
        SubmissionMode = submissionMode;
    }

    public static Activity Create(string? title, string? markdownContent, Guid topicId, Guid? sessionId, DateTimeOffset? dueDate, SubmissionMode submissionMode, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(markdownContent);

        if (topicId == Guid.Empty)
        {
            throw new ArgumentException("Topic is required.", nameof(topicId));
        }

        return new Activity(title.Trim(), markdownContent, topicId, sessionId, dueDate, submissionMode, now);
    }

    public void UpdateContent(string? title, string? markdownContent, DateTimeOffset? dueDate, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(markdownContent);
        Title = title.Trim();
        MarkdownContent = markdownContent;
        DueDate = dueDate;
        Touch(now);
    }

    public void Publish(DateTimeOffset now)
    {
        Status = ActivityStatus.Published;
        Touch(now);
    }

    public void Close(DateTimeOffset now)
    {
        Status = ActivityStatus.Closed;
        Touch(now);
    }

    /// <summary>
    /// Only published activities accept deliveries. Lateness is flagged
    /// on the submission, not rejected here (RF-15).
    /// </summary>
    public bool CanAcceptSubmissions => Status == ActivityStatus.Published;
}
