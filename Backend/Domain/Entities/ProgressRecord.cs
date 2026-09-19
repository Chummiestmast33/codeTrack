using Backend.Domain.Enums;
using Backend.Domain.Rules;

namespace Backend.Domain.Entities;

/// <summary>Progress of a student per topic; manual adjustment wins until removed (D-05, RF-20).</summary>
public sealed class ProgressRecord : AuditableEntity
{
    public Guid UserId { get; private set; }

    public Guid TopicId { get; private set; }

    public ProgressStatus AutoStatus { get; private set; } = ProgressStatus.NotStarted;

    public ProgressStatus? ManualStatus { get; private set; }

    public string? AdjustmentReason { get; private set; }

    private ProgressRecord()
    {
    }

    private ProgressRecord(Guid userId, Guid topicId, DateTimeOffset now)
        : base(now)
    {
        UserId = userId;
        TopicId = topicId;
    }

    public static ProgressRecord Create(Guid userId, Guid topicId, DateTimeOffset now)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User is required.", nameof(userId));
        }

        if (topicId == Guid.Empty)
        {
            throw new ArgumentException("Topic is required.", nameof(topicId));
        }

        return new ProgressRecord(userId, topicId, now);
    }

    public ProgressStatus EffectiveStatus => ManualStatus ?? AutoStatus;

    public void ApplyAutomatic(bool hasAttendance, bool hasReviewedActivity, DateTimeOffset now)
    {
        AutoStatus = ProgressCalculator.Calculate(hasAttendance, hasReviewedActivity);
        Touch(now);
    }

    public void AdjustManually(ProgressStatus status, string? reason, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        ManualStatus = status;
        AdjustmentReason = reason.Trim();
        Touch(now);
    }

    public void ClearAdjustment(DateTimeOffset now)
    {
        ManualStatus = null;
        AdjustmentReason = null;
        Touch(now);
    }
}
