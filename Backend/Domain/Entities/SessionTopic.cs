namespace Backend.Domain.Entities;

/// <summary>Session-topic link; unique per pair (SessionId, TopicId).</summary>
public sealed class SessionTopic
{
    public Guid SessionId { get; }

    public Guid TopicId { get; }

    public SessionTopic(Guid sessionId, Guid topicId)
    {
        if (sessionId == Guid.Empty)
        {
            throw new ArgumentException("Session is required.", nameof(sessionId));
        }

        if (topicId == Guid.Empty)
        {
            throw new ArgumentException("Topic is required.", nameof(topicId));
        }

        SessionId = sessionId;
        TopicId = topicId;
    }
}
