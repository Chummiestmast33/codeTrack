using Backend.Application.Abstractions;
using Backend.Application.Common;
using MediatR;

namespace Backend.Application.Features.Topics;

public sealed record DeactivateTopicCommand(Guid TopicId) : IRequest<TopicDto>;

public sealed class DeactivateTopicHandler : IRequestHandler<DeactivateTopicCommand, TopicDto>
{
    private readonly ITopicRepository _topics;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _time;

    public DeactivateTopicHandler(ITopicRepository topics, IUnitOfWork unitOfWork, TimeProvider time)
    {
        _topics = topics;
        _unitOfWork = unitOfWork;
        _time = time;
    }

    public async Task<TopicDto> Handle(DeactivateTopicCommand request, CancellationToken cancellationToken)
    {
        var topic = await _topics.GetByIdAsync(request.TopicId, cancellationToken)
            ?? throw new NotFoundException("Topic", request.TopicId);

        topic.Deactivate(_time.GetUtcNow());
        _topics.Update(topic);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return TopicDto.From(topic);
    }
}
