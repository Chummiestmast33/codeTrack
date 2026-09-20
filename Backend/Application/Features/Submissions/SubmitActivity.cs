using Backend.Application.Abstractions;
using Backend.Application.Common;
using Backend.Domain.Entities;
using Backend.Domain.Rules;
using FluentValidation;
using MediatR;

namespace Backend.Application.Features.Submissions;

/// <summary>Step 2: confirm the delivery after the direct upload (D-03, RF-15).</summary>
public sealed record SubmitActivityCommand(
    Guid ActivityId,
    Guid UserId,
    string? Url,
    string? StoragePath,
    string? FileName,
    string? ContentType,
    long? FileSizeBytes,
    string? Comment) : IRequest<SubmissionDto>;

public sealed class SubmitActivityValidator : AbstractValidator<SubmitActivityCommand>
{
    public SubmitActivityValidator()
    {
        RuleFor(x => x.ActivityId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public sealed class SubmitActivityHandler : IRequestHandler<SubmitActivityCommand, SubmissionDto>
{
    private readonly IActivityRepository _activities;
    private readonly ISubmissionRepository _submissions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SubmissionEnricher _enricher;
    private readonly TimeProvider _time;

    public SubmitActivityHandler(
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

    public async Task<SubmissionDto> Handle(SubmitActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = await _activities.GetByIdAsync(request.ActivityId, cancellationToken)
            ?? throw new NotFoundException("Activity", request.ActivityId);

        if (!activity.CanAcceptSubmissions)
        {
            throw new ConflictException("The activity is not open for submissions.");
        }

        var now = _time.GetUtcNow();
        var latest = await _submissions.GetLatestAsync(request.ActivityId, request.UserId, cancellationToken);

        Submission submission = latest is null
            ? Submission.CreateFirst(
                request.ActivityId, request.UserId, activity.SubmissionMode,
                request.Url, request.StoragePath, now,
                request.FileName, request.ContentType, request.FileSizeBytes, request.Comment)
            : latest.CreateNextVersion(
                activity.SubmissionMode, request.Url, request.StoragePath, now,
                request.FileName, request.ContentType, request.FileSizeBytes, request.Comment);

        await _submissions.AddAsync(submission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _enricher.ToDto(submission, activity.DueDate, controlNumber: null, fullName: null);
    }
}
