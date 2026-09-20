using Backend.Application.Abstractions;
using Backend.Application.Common;
using MediatR;

namespace Backend.Application.Features.Topics;

public sealed record GetTopicsQuery : IRequest<IReadOnlyList<TopicDto>>;

public sealed class GetTopicsHandler : IRequestHandler<GetTopicsQuery, IReadOnlyList<TopicDto>>
{
    private readonly ITopicRepository _topics;

    public GetTopicsHandler(ITopicRepository topics)
    {
        _topics = topics;
    }

    public async Task<IReadOnlyList<TopicDto>> Handle(GetTopicsQuery request, CancellationToken cancellationToken)
    {
        var topics = await _topics.ListOrderedAsync(cancellationToken);
        return topics.Select(TopicDto.From).ToList();
    }
}

public sealed record GetTopicByIdQuery(Guid TopicId) : IRequest<TopicDto>;

public sealed class GetTopicByIdHandler : IRequestHandler<GetTopicByIdQuery, TopicDto>
{
    private readonly ITopicRepository _topics;

    public GetTopicByIdHandler(ITopicRepository topics)
    {
        _topics = topics;
    }

    public async Task<TopicDto> Handle(GetTopicByIdQuery request, CancellationToken cancellationToken)
    {
        var topic = await _topics.GetByIdAsync(request.TopicId, cancellationToken)
            ?? throw new NotFoundException("Topic", request.TopicId);

        return TopicDto.From(topic);
    }
}
