using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Backend.Application.Features.Submissions;
using Backend.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Authorize]
public sealed class SubmissionsController : ControllerBase
{
    private readonly ISender _sender;

    public SubmissionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("api/activities/{activityId:guid}/upload-ticket")]
    public async Task<ActionResult<UploadTicketDto>> UploadTicket(
        Guid activityId, UploadTicketRequest request, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(
            new RequestUploadTicketCommand(activityId, CurrentUserId(), request.FileName, request.ContentType, request.FileSizeBytes),
            cancellationToken));

    [HttpPost("api/activities/{activityId:guid}/submissions")]
    public async Task<ActionResult<SubmissionDto>> Submit(
        Guid activityId, SubmitRequest request, CancellationToken cancellationToken)
    {
        var submission = await _sender.Send(
            new SubmitActivityCommand(
                activityId, CurrentUserId(), request.Url, request.StoragePath,
                request.FileName, request.ContentType, request.FileSizeBytes, request.Comment),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, submission);
    }

    [HttpGet("api/activities/{activityId:guid}/submissions/me")]
    public async Task<ActionResult<IReadOnlyList<SubmissionDto>>> Mine(Guid activityId, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetMySubmissionHistoryQuery(activityId, CurrentUserId()), cancellationToken));

    [HttpGet("api/admin/activities/{activityId:guid}/submissions")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<IReadOnlyList<SubmissionDto>>> ByActivity(Guid activityId, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetSubmissionsByActivityQuery(activityId), cancellationToken));

    [HttpPatch("api/admin/submissions/{id:guid}/status")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<SubmissionDto>> Review(Guid id, ReviewRequest request, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(
            new ReviewSubmissionCommand(id, request.Status, request.InstructorComment, CurrentUserId()),
            cancellationToken));

    private Guid CurrentUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}

public sealed record UploadTicketRequest(
    [Required] string FileName,
    [Required] string ContentType,
    [Range(1, long.MaxValue)] long FileSizeBytes);

public sealed record SubmitRequest(
    string? Url,
    string? StoragePath,
    string? FileName,
    string? ContentType,
    long? FileSizeBytes,
    string? Comment);

public sealed record ReviewRequest(
    SubmissionStatus Status,
    string? InstructorComment);
