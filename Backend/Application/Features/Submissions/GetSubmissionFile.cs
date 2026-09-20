using Backend.Application.Abstractions;
using Backend.Application.Common;
using Backend.Domain.Enums;
using MediatR;

namespace Backend.Application.Features.Submissions;

public sealed record GetSubmissionFileQuery(Guid SubmissionId, Guid RequesterId) : IRequest<SubmissionFileDto>;

public sealed record SubmissionFileDto(string DownloadUrl, string FileName);

public sealed class GetSubmissionFileHandler(
    ISubmissionRepository submissions,
    IUserRepository users,
    IFileStorage storage) : IRequestHandler<GetSubmissionFileQuery, SubmissionFileDto>
{
    public async Task<SubmissionFileDto> Handle(GetSubmissionFileQuery request, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(request.RequesterId, cancellationToken);
        if (user is null || user.Role != UserRole.Administrator || !user.CanSignIn)
        {
            throw new ForbiddenException("Only active, approved administrators can access submission files.");
        }

        var submission = await submissions.GetByIdAsync(request.SubmissionId, cancellationToken)
            ?? throw new NotFoundException("Submission", request.SubmissionId);
        if (string.IsNullOrWhiteSpace(submission.StoragePath))
        {
            throw new NotFoundException("Submission file", request.SubmissionId);
        }

        var url = await storage.GetDownloadUrlAsync(submission.StoragePath, cancellationToken);
        return new SubmissionFileDto(url, submission.FileName ?? "submission");
    }
}
