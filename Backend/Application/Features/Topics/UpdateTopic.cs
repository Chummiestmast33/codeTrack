using Backend.Application.Abstractions;
using Backend.Application.Common;
using FluentValidation;
using MediatR;

namespace Backend.Application.Features.Topics;

public sealed record UpdateTopicCommand(Guid TopicId, string Name, string? Description, int OrderNumber) : IRequest<TopicDto>;

public sealed class UpdateTopicValidator : AbstractValidator<UpdateTopicCommand>
{
    public UpdateTopicValidator()
    {
        RuleFor(x => x.TopicId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OrderNumber).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateTopicHandler : IRequestHandler<UpdateTopicCommand, TopicDto>
{
    private readonly ITopicRepository _topics;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _time;

    public UpdateTopicHandler(ITopicRepository topics, IUnitOfWork unitOfWork, TimeProvider time)
    {
        _topics = topics;
        _unitOfWork = unitOfWork;
        _time = time;
    }

    public async Task<TopicDto> Handle(UpdateTopicCommand request, CancellationToken cancellationToken)
    {
        var topic = await _topics.GetByIdAsync(request.TopicId, cancellationToken)
            ?? throw new NotFoundException("Topic", request.TopicId);

        topic.Rename(request.Name, request.Description, _time.GetUtcNow());
        topic.Reorder(request.OrderNumber, _time.GetUtcNow());
        _topics.Update(topic);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return TopicDto.From(topic);
    }
}
