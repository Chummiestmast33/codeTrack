using Backend.Application.Abstractions;
using Backend.Domain.Entities;

namespace Backend.Tests.Features.Catalog;

public sealed class FakeTopicRepository : ITopicRepository
{
    private readonly Dictionary<Guid, Topic> _topics = [];

    public Task<Topic?> GetByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(_topics.GetValueOrDefault(id));

    public Task<IReadOnlyList<Topic>> ListOrderedAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Topic>>(_topics.Values.OrderBy(t => t.OrderNumber).ToList());

    public Task AddAsync(Topic topic, CancellationToken ct)
    {
        _topics[topic.Id] = topic;
        return Task.CompletedTask;
    }

    public void Update(Topic topic) => _topics[topic.Id] = topic;

    public void Seed(Topic topic) => _topics[topic.Id] = topic;
}

public sealed class FakeSessionRepository : ISessionRepository
{
    private readonly Dictionary<Guid, Session> _sessions = [];
    private readonly Dictionary<Guid, List<Guid>> _links = [];

    public Task<Session?> GetByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(_sessions.GetValueOrDefault(id));

    public Task<IReadOnlyList<Session>> ListAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Session>>(_sessions.Values.OrderBy(s => s.SessionDate).ToList());

    public Task<IReadOnlyList<Guid>> GetTopicIdsAsync(Guid sessionId, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Guid>>(_links.GetValueOrDefault(sessionId, []));

    public Task<IReadOnlyList<Guid>> GetSessionIdsByTopicAsync(Guid topicId, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Guid>>(_links.Where(kv => kv.Value.Contains(topicId)).Select(kv => kv.Key).ToList());

    public Task ReplaceTopicsAsync(Guid sessionId, IReadOnlyList<Guid> topicIds, CancellationToken ct)
    {
        _links[sessionId] = topicIds.Distinct().ToList();
        return Task.CompletedTask;
    }

    public Task AddAsync(Session session, CancellationToken ct)
    {
        _sessions[session.Id] = session;
        return Task.CompletedTask;
    }

    public void Update(Session session) => _sessions[session.Id] = session;

    public void Seed(Session session, IEnumerable<Guid>? topicIds = null)
    {
        _sessions[session.Id] = session;
        if (topicIds is not null)
        {
            _links[session.Id] = topicIds.Distinct().ToList();
        }
    }
}

public sealed class FakeAttendanceRepository : IAttendanceRepository
{
    private readonly Dictionary<Guid, AttendanceRecord> _records = [];

    public Task<AttendanceRecord?> GetByIdAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(_records.GetValueOrDefault(id));

    public Task<AttendanceRecord?> GetAsync(Guid sessionId, Guid userId, CancellationToken ct) =>
        Task.FromResult(_records.Values.FirstOrDefault(a => a.SessionId == sessionId && a.UserId == userId));

    public Task<IReadOnlyList<AttendanceRecord>> ListBySessionAsync(Guid sessionId, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<AttendanceRecord>>(
            _records.Values.Where(a => a.SessionId == sessionId).OrderBy(a => a.CreatedAt).ToList());

    public Task<IReadOnlyList<AttendanceRecord>> ListByUserAsync(Guid userId, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<AttendanceRecord>>(
            _records.Values.Where(a => a.UserId == userId).OrderBy(a => a.CreatedAt).ToList());

    public Task AddAsync(AttendanceRecord record, CancellationToken ct)
    {
        _records[record.Id] = record;
        return Task.CompletedTask;
    }

    public void Update(AttendanceRecord record) => _records[record.Id] = record;

    public void SeedFor(Guid sessionId, Guid userId)
    {
        var record = AttendanceRecord.Register(
            sessionId, userId, Domain.Enums.AttendanceStatus.Present, userId,
            new DateTimeOffset(2026, 9, 19, 2, 0, 0, TimeSpan.Zero));
        _records[record.Id] = record;
    }
}

public sealed class FakeQrTokenRepository : IQrTokenRepository
{
    private readonly List<QrToken> _tokens = [];

    public Task<QrToken?> GetByTokenAsync(string token, CancellationToken ct) =>
        Task.FromResult(_tokens.FirstOrDefault(q => q.Token == token));

    public Task<QrToken?> GetLatestBySessionAsync(Guid sessionId, CancellationToken ct) =>
        Task.FromResult(_tokens.Where(q => q.SessionId == sessionId).OrderByDescending(q => q.CreatedAt).FirstOrDefault());

    public Task AddAsync(QrToken token, CancellationToken ct)
    {
        _tokens.Add(token);
        return Task.CompletedTask;
    }
}
