using Backend.Domain.Entities;

namespace Backend.Application.Abstractions;

public interface ISessionRepository
{
    Task<Session?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Session>> ListAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Guid>> GetTopicIdsAsync(Guid sessionId, CancellationToken cancellationToken);

    Task ReplaceTopicsAsync(Guid sessionId, IReadOnlyList<Guid> topicIds, CancellationToken cancellationToken);

    Task AddAsync(Session session, CancellationToken cancellationToken);

    void Update(Session session);
}
