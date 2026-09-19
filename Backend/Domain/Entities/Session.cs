using Backend.Domain.Enums;

namespace Backend.Domain.Entities;

/// <summary>Workshop session linked to one or more topics (RF-06).</summary>
public sealed class Session : AuditableEntity
{
    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public DateTimeOffset SessionDate { get; private set; }

    public SessionStatus Status { get; private set; } = SessionStatus.Planned;

    public Guid CreatedBy { get; private set; }

    private Session()
    {
    }

    private Session(string title, string? description, DateTimeOffset sessionDate, Guid createdBy, DateTimeOffset now)
        : base(now)
    {
        Title = title;
        Description = description;
        SessionDate = sessionDate;
        CreatedBy = createdBy;
    }

    public static Session Create(string? title, string? description, DateTimeOffset sessionDate, Guid createdBy, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        if (createdBy == Guid.Empty)
        {
            throw new ArgumentException("Creator is required.", nameof(createdBy));
        }

        return new Session(title.Trim(), description, sessionDate, createdBy, now);
    }

    public void MarkImparted(DateTimeOffset now)
    {
        Status = SessionStatus.Imparted;
        Touch(now);
    }

    public void Cancel(DateTimeOffset now)
    {
        Status = SessionStatus.Cancelled;
        Touch(now);
    }
}
