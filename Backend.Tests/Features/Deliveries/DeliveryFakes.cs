using Backend.Application.Abstractions;
using Backend.Domain.Entities;

namespace Backend.Tests.Features.Deliveries;

public sealed class FakeActivityRepository : IActivityRepository
{
    private readonly Dictionary<Guid, Activity> _activities = [];

    public Task<Activity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(_activities.GetValueOrDefault(id));

    public Task<IReadOnlyList<Activity>> ListAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Activity>>(_activities.Values.OrderBy(a => a.CreatedAt).ToList());

    public Task<IReadOnlyList<Activity>> ListByTopicAsync(Guid topicId, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Activity>>(_activities.Values.Where(a => a.TopicId == topicId).ToList());

    public Task AddAsync(Activity activity, CancellationToken ct)
    {
        _activities[activity.Id] = activity;
        return Task.CompletedTask;
    }

    public void Update(Activity activity) => _activities[activity.Id] = activity;

    public void Seed(Activity activity) => _activities[activity.Id] = activity;
}

public sealed class FakeSubmissionRepository : ISubmissionRepository
{
    private readonly List<Submission> _submissions = [];

    public Task<Submission?> GetByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(_submissions.FirstOrDefault(s => s.Id == id));

    public Task<Submission?> GetLatestAsync(Guid activityId, Guid userId, CancellationToken ct) =>
        Task.FromResult(_submissions
            .Where(s => s.ActivityId == activityId && s.UserId == userId)
            .OrderByDescending(s => s.VersionNumber)
            .FirstOrDefault());

    public Task<IReadOnlyList<Submission>> ListHistoryAsync(Guid activityId, Guid userId, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Submission>>(_submissions
            .Where(s => s.ActivityId == activityId && s.UserId == userId)
            .OrderBy(s => s.VersionNumber)
            .ToList());

    public Task<IReadOnlyList<Submission>> ListByActivityAsync(Guid activityId, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Submission>>(_submissions.Where(s => s.ActivityId == activityId).ToList());

    public Task<IReadOnlyList<Submission>> ListByUserAsync(Guid userId, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Submission>>(_submissions.Where(s => s.UserId == userId).ToList());

    public Task AddAsync(Submission submission, CancellationToken ct)
    {
        _submissions.Add(submission);
        return Task.CompletedTask;
    }

    public void Update(Submission submission)
    {
        var index = _submissions.FindIndex(s => s.Id == submission.Id);
        if (index >= 0)
        {
            _submissions[index] = submission;
        }
    }
}

public sealed class FakeProgressRepository : IProgressRepository
{
    private readonly Dictionary<(Guid, Guid), ProgressRecord> _records = [];

    public Task<ProgressRecord?> GetAsync(Guid userId, Guid topicId, CancellationToken ct) =>
        Task.FromResult(_records.GetValueOrDefault((userId, topicId)));

    public Task<IReadOnlyList<ProgressRecord>> ListByUserAsync(Guid userId, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<ProgressRecord>>(_records.Values.Where(p => p.UserId == userId).ToList());

    public Task AddAsync(ProgressRecord record, CancellationToken ct)
    {
        _records[(record.UserId, record.TopicId)] = record;
        return Task.CompletedTask;
    }

    public void Update(ProgressRecord record) => _records[(record.UserId, record.TopicId)] = record;
}
