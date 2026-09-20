using Backend.Application.Abstractions;
using Backend.Application.Common;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Backend.Application.Features.Submissions;

public sealed class SubmissionEnricher
{
    public SubmissionDto ToDto(Submission submission, DateTimeOffset? dueDate, string? controlNumber, string? fullName) => new(
        submission.Id, submission.ActivityId, submission.UserId,
        controlNumber, fullName,
        submission.Url, submission.FileName, submission.ContentType, submission.FileSizeBytes,
        submission.StoragePath, submission.Comment, submission.InstructorComment,
        submission.Status, submission.VersionNumber, submission.SubmittedAt,
        submission.IsLate(dueDate), submission.ReviewedAt);
}

public sealed record GetMySubmissionHistoryQuery(Guid ActivityId, Guid UserId) : IRequest<IReadOnlyList<SubmissionDto>>;

public sealed class GetMySubmissionHistoryHandler : IRequestHandler<GetMySubmissionHistoryQuery, IReadOnlyList<SubmissionDto>>
{
    private readonly IActivityRepository _activities;
    private readonly ISubmissionRepository _submissions;
    private readonly SubmissionEnricher _enricher;

    public GetMySubmissionHistoryHandler(
        IActivityRepository activities,
        ISubmissionRepository submissions,
        SubmissionEnricher enricher)
    {
        _activities = activities;
        _submissions = submissions;
        _enricher = enricher;
    }

    public async Task<IReadOnlyList<SubmissionDto>> Handle(GetMySubmissionHistoryQuery request, CancellationToken cancellationToken)
    {
        var activity = await _activities.GetByIdAsync(request.ActivityId, cancellationToken)
            ?? throw new NotFoundException("Activity", request.ActivityId);

        var history = await _submissions.ListHistoryAsync(request.ActivityId, request.UserId, cancellationToken);
        return history.Select(s => _enricher.ToDto(s, activity.DueDate, null, null)).ToList();
    }
}

public sealed record GetSubmissionsByActivityQuery(Guid ActivityId) : IRequest<IReadOnlyList<SubmissionDto>>;

public sealed class GetSubmissionsByActivityHandler : IRequestHandler<GetSubmissionsByActivityQuery, IReadOnlyList<SubmissionDto>>
{
    private readonly IActivityRepository _activities;
    private readonly ISubmissionRepository _submissions;
    private readonly IUserRepository _users;
    private readonly SubmissionEnricher _enricher;

    public GetSubmissionsByActivityHandler(
        IActivityRepository activities,
        ISubmissionRepository submissions,
        IUserRepository users,
        SubmissionEnricher enricher)
    {
        _activities = activities;
        _submissions = submissions;
        _users = users;
        _enricher = enricher;
    }

    public async Task<IReadOnlyList<SubmissionDto>> Handle(GetSubmissionsByActivityQuery request, CancellationToken cancellationToken)
    {
        var activity = await _activities.GetByIdAsync(request.ActivityId, cancellationToken)
            ?? throw new NotFoundException("Activity", request.ActivityId);

        var submissions = await _submissions.ListByActivityAsync(request.ActivityId, cancellationToken);
        var result = new List<SubmissionDto>(submissions.Count);
        foreach (var submission in submissions)
        {
            var user = await _users.GetByIdAsync(submission.UserId, cancellationToken);
            result.Add(_enricher.ToDto(submission, activity.DueDate, user?.ControlNumber, user?.FullName));
        }

        return result;
    }
}

public sealed record ReviewSubmissionCommand(
    Guid SubmissionId,
    SubmissionStatus Status,
    string? InstructorComment,
    Guid ReviewerId) : IRequest<SubmissionDto>;

public sealed class ReviewSubmissionValidator : AbstractValidator<ReviewSubmissionCommand>
{
    public ReviewSubmissionValidator()
    {
        RuleFor(x => x.SubmissionId).NotEmpty();
        RuleFor(x => x.ReviewerId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}

public sealed class ReviewSubmissionHandler : IRequestHandler<ReviewSubmissionCommand, SubmissionDto>
{
    private readonly IActivityRepository _activities;
    private readonly ISubmissionRepository _submissions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SubmissionEnricher _enricher;
    private readonly TimeProvider _time;

    public ReviewSubmissionHandler(
        IActivityRepository activities,
        ISubmissionRepository submissions,
        IUnitOfWork unitOfWork,
        SubmissionEnricher enricher,
        TimeProvider time)
    {
        _activities = activities;
        _submissions = submissions;
        _unitOfWork = unitOfWork;
        _enricher = enricher;
        _time = time;
    }

    public async Task<SubmissionDto> Handle(ReviewSubmissionCommand request, CancellationToken cancellationToken)
    {
        var submission = await _submissions.GetByIdAsync(request.SubmissionId, cancellationToken)
            ?? throw new NotFoundException("Submission", request.SubmissionId);

        var activity = await _activities.GetByIdAsync(submission.ActivityId, cancellationToken)
            ?? throw new NotFoundException("Activity", submission.ActivityId);

        submission.Review(request.ReviewerId, request.Status, _time.GetUtcNow(), request.InstructorComment);
        _submissions.Update(submission);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _enricher.ToDto(submission, activity.DueDate, null, null);
    }
}
