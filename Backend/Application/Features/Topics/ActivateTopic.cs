using Backend.Application.Abstractions;
using Backend.Application.Common;
using MediatR;

namespace Backend.Application.Features.Topics;

public sealed record ActivateTopicCommand(Guid TopicId) : IRequest<TopicDto>;

public sealed class ActivateTopicHandler : IRequestHandler<ActivateTopicCommand, TopicDto>
{
    private readonly ITopicRepository _topics;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _time;

    public ActivateTopicHandler(ITopicRepository topics, IUnitOfWork unitOfWork, TimeProvider time)
    {
        _topics = topics;
        _unitOfWork = unitOfWork;
        _time = time;
    }

    public async Task<TopicDto> Handle(ActivateTopicCommand request, CancellationToken cancellationToken)
    {
        var topic = await _topics.GetByIdAsync(request.TopicId, cancellationToken)
            ?? throw new NotFoundException("Topic", request.TopicId);

        topic.Activate(_time.GetUtcNow());
        _topics.Update(topic);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return TopicDto.From(topic);
    }
}
