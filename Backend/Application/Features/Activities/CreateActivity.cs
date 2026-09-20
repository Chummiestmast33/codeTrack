using Backend.Application.Abstractions;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Backend.Application.Features.Activities;

/// <summary>Instructor creates an activity (RF-12, RF-14).</summary>
public sealed record CreateActivityCommand(
    string Title,
    string MarkdownContent,
    Guid TopicId,
    Guid? SessionId,
    DateTimeOffset? DueDate,
    SubmissionMode SubmissionMode) : IRequest<ActivityDto>;

public sealed class CreateActivityValidator : AbstractValidator<CreateActivityCommand>
{
    public CreateActivityValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MarkdownContent).NotEmpty();
        RuleFor(x => x.TopicId).NotEmpty();
        RuleFor(x => x.SubmissionMode).IsInEnum();
    }
}

public sealed class CreateActivityHandler : IRequestHandler<CreateActivityCommand, ActivityDto>
{
    private readonly IActivityRepository _activities;
    private readonly ITopicRepository _topics;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ActivityEnricher _enricher;
    private readonly TimeProvider _time;

    public CreateActivityHandler(
        IActivityRepository activities,
        ITopicRepository topics,
        IUnitOfWork unitOfWork,
        ActivityEnricher enricher,
        TimeProvider time)
    {
        _activities = activities;
        _topics = topics;
        _unitOfWork = unitOfWork;
        _enricher = enricher;
        _time = time;
    }

    public async Task<ActivityDto> Handle(CreateActivityCommand request, CancellationToken cancellationToken)
    {
        var topic = await _topics.GetByIdAsync(request.TopicId, cancellationToken)
            ?? throw new Common.NotFoundException("Topic", request.TopicId);

        var activity = Activity.Create(
            request.Title, request.MarkdownContent, request.TopicId,
            request.SessionId, request.DueDate, request.SubmissionMode, _time.GetUtcNow());

        await _activities.AddAsync(activity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _enricher.ToDto(activity, topic.Name);
    }
}
