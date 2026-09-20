using Backend.Application.Abstractions;
using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Persistence.Repositories;

public sealed class SessionRepository : ISessionRepository
{
    private readonly TallerDbContext _db;

    public SessionRepository(TallerDbContext db)
    {
        _db = db;
    }

    public Task<Session?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _db.Sessions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Session>> ListAsync(CancellationToken cancellationToken) =>
        await _db.Sessions.OrderBy(s => s.SessionDate).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetTopicIdsAsync(Guid sessionId, CancellationToken cancellationToken) =>
        await _db.SessionTopics
            .Where(st => st.SessionId == sessionId)
            .Select(st => st.TopicId)
            .ToListAsync(cancellationToken);

    public async Task ReplaceTopicsAsync(Guid sessionId, IReadOnlyList<Guid> topicIds, CancellationToken cancellationToken)
    {
        var existing = await _db.SessionTopics
            .Where(st => st.SessionId == sessionId)
            .ToListAsync(cancellationToken);
        _db.SessionTopics.RemoveRange(existing);

        foreach (var topicId in topicIds.Distinct())
        {
            await _db.SessionTopics.AddAsync(new SessionTopic(sessionId, topicId), cancellationToken);
        }
    }

    public async Task AddAsync(Session session, CancellationToken cancellationToken) =>
        await _db.Sessions.AddAsync(session, cancellationToken);

    public void Update(Session session) => _db.Sessions.Update(session);
}
