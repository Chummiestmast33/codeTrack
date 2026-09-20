using Backend.Application.Abstractions;
using Backend.Application.Common;
using Backend.Domain.Rules;
using FluentValidation;
using MediatR;

namespace Backend.Application.Features.Submissions;

/// <summary>Step 1 of the direct-upload flow: validate and issue a signed URL (RNF-17).</summary>
public sealed record RequestUploadTicketCommand(
    Guid ActivityId,
    Guid UserId,
    string FileName,
    string ContentType,
    long FileSizeBytes) : IRequest<UploadTicketDto>;

public sealed record UploadTicketDto(string UploadUrl, string StoragePath, DateTimeOffset ExpiresAt, int VersionNumber);

public sealed class RequestUploadTicketValidator : AbstractValidator<RequestUploadTicketCommand>
{
    public RequestUploadTicketValidator()
    {
        RuleFor(x => x.ActivityId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty();
        RuleFor(x => x.ContentType).NotEmpty();
        RuleFor(x => x.FileSizeBytes).GreaterThan(0)
            .LessThanOrEqualTo(SubmissionPolicy.MaxFileSizeBytes);
    }
}

public sealed class RequestUploadTicketHandler : IRequestHandler<RequestUploadTicketCommand, UploadTicketDto>
{
    private readonly IActivityRepository _activities;
    private readonly ISubmissionRepository _submissions;
    private readonly IFileStorage _storage;

    public RequestUploadTicketHandler(
        IActivityRepository activities,
        ISubmissionRepository submissions,
        IFileStorage storage)
    {
        _activities = activities;
        _submissions = submissions;
        _storage = storage;
    }

    public async Task<UploadTicketDto> Handle(RequestUploadTicketCommand request, CancellationToken cancellationToken)
    {
        var activity = await _activities.GetByIdAsync(request.ActivityId, cancellationToken)
            ?? throw new NotFoundException("Activity", request.ActivityId);

        if (!activity.CanAcceptSubmissions)
        {
            throw new ConflictException("The activity is not open for submissions.");
        }

        SubmissionPolicy.EnsureExtension(request.FileName);

        var latest = await _submissions.GetLatestAsync(request.ActivityId, request.UserId, cancellationToken);
        var version = SubmissionPolicy.NextVersionNumber(latest?.VersionNumber ?? 0);
        var key = SubmissionPolicy.BuildObjectKey(request.ActivityId, request.UserId, version, request.FileName);

        var ticket = await _storage.GetUploadUrlAsync(key, request.ContentType, request.FileSizeBytes, cancellationToken);
        return new UploadTicketDto(ticket.UploadUrl, ticket.StoragePath, ticket.ExpiresAt, version);
    }
}
