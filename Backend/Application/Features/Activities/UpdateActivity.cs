using Backend.Application.Abstractions;
using Backend.Application.Common;
using Backend.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Backend.Application.Features.Activities;

public sealed record UpdateActivityCommand(
    Guid ActivityId,
    string Title,
    string MarkdownContent,
    DateTimeOffset? DueDate) : IRequest<ActivityDto>;

public sealed class UpdateActivityValidator : AbstractValidator<UpdateActivityCommand>
{
    public UpdateActivityValidator()
    {
        RuleFor(x => x.ActivityId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MarkdownContent).NotEmpty();
    }
}

public sealed class UpdateActivityHandler : IRequestHandler<UpdateActivityCommand, ActivityDto>
{
    private readonly IActivityRepository _activities;
    private readonly ITopicRepository _topics;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ActivityEnricher _enricher;
    private readonly TimeProvider _time;

    public UpdateActivityHandler(
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

    public async Task<ActivityDto> Handle(UpdateActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = await _activities.GetByIdAsync(request.ActivityId, cancellationToken)
            ?? throw new NotFoundException("Activity", request.ActivityId);

        activity.UpdateContent(request.Title, request.MarkdownContent, request.DueDate, _time.GetUtcNow());
        _activities.Update(activity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var topic = await _topics.GetByIdAsync(activity.TopicId, cancellationToken)
            ?? throw new NotFoundException("Topic", activity.TopicId);
        return _enricher.ToDto(activity, topic.Name);
    }
}
