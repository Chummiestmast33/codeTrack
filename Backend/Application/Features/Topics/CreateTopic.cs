using Backend.Application.Abstractions;
using Backend.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Backend.Application.Features.Topics;

/// <summary>Instructor creates a topic (RF-05).</summary>
public sealed record CreateTopicCommand(string Name, string? Description, int OrderNumber) : IRequest<TopicDto>;

public sealed class CreateTopicValidator : AbstractValidator<CreateTopicCommand>
{
    public CreateTopicValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OrderNumber).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateTopicHandler : IRequestHandler<CreateTopicCommand, TopicDto>
{
    private readonly ITopicRepository _topics;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _time;

    public CreateTopicHandler(ITopicRepository topics, IUnitOfWork unitOfWork, TimeProvider time)
    {
        _topics = topics;
        _unitOfWork = unitOfWork;
        _time = time;
    }

    public async Task<TopicDto> Handle(CreateTopicCommand request, CancellationToken cancellationToken)
    {
        var topic = Topic.Create(request.Name, request.Description, request.OrderNumber, _time.GetUtcNow());
        await _topics.AddAsync(topic, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return TopicDto.From(topic);
    }
}
