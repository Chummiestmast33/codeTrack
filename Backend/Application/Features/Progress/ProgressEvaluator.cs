using Backend.Application.Abstractions;
using Backend.Domain.Enums;
using Backend.Domain.Rules;

namespace Backend.Application.Features.Progress;

/// <summary>Computes automatic progress on read (D-05); records persist manual overrides only.</summary>
public sealed class ProgressEvaluator
{
    private readonly ITopicRepository _topics;
    private readonly ISessionRepository _sessions;
    private readonly IActivityRepository _activities;
    private readonly IAttendanceRepository _attendance;
    private readonly ISubmissionRepository _submissions;
    private readonly IProgressRepository _progress;

    public ProgressEvaluator(
        ITopicRepository topics,
        ISessionRepository sessions,
        IActivityRepository activities,
        IAttendanceRepository attendance,
        ISubmissionRepository submissions,
        IProgressRepository progress)
    {
        _topics = topics;
        _sessions = sessions;
        _activities = activities;
        _attendance = attendance;
        _submissions = submissions;
        _progress = progress;
    }

    public async Task<IReadOnlyList<TopicProgressDto>> EvaluateAsync(Guid userId, CancellationToken cancellationToken)
    {
        var topics = await _topics.ListOrderedAsync(cancellationToken);
        var records = await _attendance.ListByUserAsync(userId, cancellationToken);
        var submissions = await _submissions.ListByUserAsync(userId, cancellationToken);
        var overrides = (await _progress.ListByUserAsync(userId, cancellationToken))
            .ToDictionary(p => p.TopicId);

        var result = new List<TopicProgressDto>(topics.Count);
        foreach (var topic in topics.Where(t => t.IsActive))
        {
            var sessionIds = (await _sessions.GetSessionIdsByTopicAsync(topic.Id, cancellationToken)).ToHashSet();
            var activityIds = (await _activities.ListByTopicAsync(topic.Id, cancellationToken))
                .Select(a => a.Id).ToHashSet();

            var hasAttendance = records.Any(a => sessionIds.Contains(a.SessionId));
            var hasReviewed = submissions.Any(s =>
                activityIds.Contains(s.ActivityId) && s.Status == SubmissionStatus.Reviewed);
            var auto = ProgressCalculator.Calculate(hasAttendance, hasReviewedActivity: hasReviewed);

            overrides.TryGetValue(topic.Id, out var record);
            // Manual adjustment wins until removed; otherwise the freshly computed auto status rules.
            // Stored AutoStatus is never used: automatic progress is always calculated on read.
            var effective = record?.ManualStatus ?? auto;
            result.Add(new TopicProgressDto(
                topic.Id, topic.Name, auto, effective,
                record?.ManualStatus, record?.AdjustmentReason,
                hasAttendance, hasReviewed));
        }

        return result;
    }
}
