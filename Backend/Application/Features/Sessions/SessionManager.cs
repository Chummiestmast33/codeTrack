using Backend.Application.Abstractions;
using Backend.Application.Common;

namespace Backend.Application.Features.Sessions;

/// <summary>Shared session persistence: topic validation, link sync and DTO loading.</summary>
public sealed class SessionManager
{
    private readonly ISessionRepository _sessions;
    private readonly ITopicRepository _topics;
    private readonly IUnitOfWork _unitOfWork;

    public SessionManager(ISessionRepository sessions, ITopicRepository topics, IUnitOfWork unitOfWork)
    {
        _sessions = sessions;
        _topics = topics;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<Guid>> ResolveTopicsAsync(IReadOnlyList<Guid> topicIds, CancellationToken cancellationToken)
    {
        var distinct = topicIds.Distinct().ToList();
        foreach (var id in distinct)
        {
            if (await _topics.GetByIdAsync(id, cancellationToken) is null)
            {
                throw new NotFoundException("Topic", id);
            }
        }

        return distinct;
    }

    public async Task SaveTopicsAsync(Guid sessionId, IReadOnlyList<Guid> topicIds, CancellationToken cancellationToken)
    {
        await _sessions.ReplaceTopicsAsync(sessionId, await ResolveTopicsAsync(topicIds, cancellationToken), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<SessionDto> LoadDtoAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await _sessions.GetByIdAsync(sessionId, cancellationToken)
            ?? throw new NotFoundException("Session", sessionId);
        var topics = new List<TopicRefDto>();
        foreach (var id in await _sessions.GetTopicIdsAsync(sessionId, cancellationToken))
        {
            var topic = await _topics.GetByIdAsync(id, cancellationToken);
            if (topic is not null)
            {
                topics.Add(new TopicRefDto(topic.Id, topic.Name));
            }
        }

        return new SessionDto(session.Id, session.Title, session.Description, session.SessionDate, session.Status, topics, session.CreatedAt);
    }
}
