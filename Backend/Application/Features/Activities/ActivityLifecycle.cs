using Backend.Application.Abstractions;
using Backend.Application.Common;
using Backend.Domain.Entities;
using MediatR;

namespace Backend.Application.Features.Activities;

public sealed class ActivityEnricher
{
    public ActivityDto ToDto(Activity activity, string topicName) => new(
        activity.Id, activity.Title, activity.MarkdownContent, activity.TopicId,
        topicName, activity.SessionId, activity.DueDate, activity.Status, activity.SubmissionMode);
}

public sealed record PublishActivityCommand(Guid ActivityId) : IRequest<ActivityDto>;

public sealed class PublishActivityHandler : IRequestHandler<PublishActivityCommand, ActivityDto>
{
    private readonly IActivityRepository _activities;
    private readonly ITopicRepository _topics;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ActivityEnricher _enricher;
    private readonly TimeProvider _time;

    public PublishActivityHandler(
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

    public async Task<ActivityDto> Handle(PublishActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = await _activities.GetByIdAsync(request.ActivityId, cancellationToken)
            ?? throw new NotFoundException("Activity", request.ActivityId);

        activity.Publish(_time.GetUtcNow());
        _activities.Update(activity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var topic = await _topics.GetByIdAsync(activity.TopicId, cancellationToken)
            ?? throw new NotFoundException("Topic", activity.TopicId);
        return _enricher.ToDto(activity, topic.Name);
    }
}

public sealed record CloseActivityCommand(Guid ActivityId) : IRequest<ActivityDto>;

public sealed class CloseActivityHandler : IRequestHandler<CloseActivityCommand, ActivityDto>
{
    private readonly IActivityRepository _activities;
    private readonly ITopicRepository _topics;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ActivityEnricher _enricher;
    private readonly TimeProvider _time;

    public CloseActivityHandler(
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

    public async Task<ActivityDto> Handle(CloseActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = await _activities.GetByIdAsync(request.ActivityId, cancellationToken)
            ?? throw new NotFoundException("Activity", request.ActivityId);

        activity.Close(_time.GetUtcNow());
        _activities.Update(activity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var topic = await _topics.GetByIdAsync(activity.TopicId, cancellationToken)
            ?? throw new NotFoundException("Topic", activity.TopicId);
        return _enricher.ToDto(activity, topic.Name);
    }
}

public sealed record GetActivitiesQuery(Guid? TopicId) : IRequest<IReadOnlyList<ActivityDto>>;

public sealed class GetActivitiesHandler : IRequestHandler<GetActivitiesQuery, IReadOnlyList<ActivityDto>>
{
    private readonly IActivityRepository _activities;
    private readonly ITopicRepository _topics;
    private readonly ActivityEnricher _enricher;

    public GetActivitiesHandler(
        IActivityRepository activities,
        ITopicRepository topics,
        ActivityEnricher enricher)
    {
        _activities = activities;
        _topics = topics;
        _enricher = enricher;
    }

    public async Task<IReadOnlyList<ActivityDto>> Handle(GetActivitiesQuery request, CancellationToken cancellationToken)
    {
        var activities = request.TopicId.HasValue
            ? await _activities.ListByTopicAsync(request.TopicId.Value, cancellationToken)
            : await _activities.ListAsync(cancellationToken);

        var result = new List<ActivityDto>(activities.Count);
        foreach (var activity in activities)
        {
            var topic = await _topics.GetByIdAsync(activity.TopicId, cancellationToken)
                ?? throw new NotFoundException("Topic", activity.TopicId);
            result.Add(_enricher.ToDto(activity, topic.Name));
        }

        return result;
    }
}

public sealed record GetActivityByIdQuery(Guid ActivityId) : IRequest<ActivityDto>;

public sealed class GetActivityByIdHandler : IRequestHandler<GetActivityByIdQuery, ActivityDto>
{
    private readonly IActivityRepository _activities;
    private readonly ITopicRepository _topics;
    private readonly ActivityEnricher _enricher;

    public GetActivityByIdHandler(
        IActivityRepository activities,
        ITopicRepository topics,
        ActivityEnricher enricher)
    {
        _activities = activities;
        _topics = topics;
        _enricher = enricher;
    }

    public async Task<ActivityDto> Handle(GetActivityByIdQuery request, CancellationToken cancellationToken)
    {
        var activity = await _activities.GetByIdAsync(request.ActivityId, cancellationToken)
            ?? throw new NotFoundException("Activity", request.ActivityId);

        var topic = await _topics.GetByIdAsync(activity.TopicId, cancellationToken)
            ?? throw new NotFoundException("Topic", activity.TopicId);
        return _enricher.ToDto(activity, topic.Name);
    }
}
