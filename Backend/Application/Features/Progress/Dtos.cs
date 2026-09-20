using Backend.Domain.Enums;

namespace Backend.Application.Features.Progress;

public sealed record TopicProgressDto(
    Guid TopicId,
    string TopicName,
    ProgressStatus AutoStatus,
    ProgressStatus EffectiveStatus,
    ProgressStatus? ManualStatus,
    string? AdjustmentReason,
    bool HasAttendance,
    bool HasReviewedActivity);
