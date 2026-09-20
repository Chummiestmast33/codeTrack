using Backend.Domain.Entities;

namespace Backend.Application.Features.Topics;

public sealed record TopicDto(Guid Id, string Name, string? Description, int OrderNumber, bool IsActive)
{
    public static TopicDto From(Topic topic) => new(
        topic.Id, topic.Name, topic.Description, topic.OrderNumber, topic.IsActive);
}
