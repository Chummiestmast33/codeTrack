using System.ComponentModel.DataAnnotations;
using Backend.Application.Features.Activities;
using Backend.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/activities")]
[Authorize]
public sealed class ActivitiesController : ControllerBase
{
    private readonly ISender _sender;

    public ActivitiesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ActivityDto>>> List([FromQuery] Guid? topicId, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetActivitiesQuery(topicId), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ActivityDto>> ById(Guid id, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetActivityByIdQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ActivityDto>> Create(CreateActivityRequest request, CancellationToken cancellationToken)
    {
        var activity = await _sender.Send(
            new CreateActivityCommand(
                request.Title, request.MarkdownContent, request.TopicId,
                request.SessionId, request.DueDate, request.SubmissionMode),
            cancellationToken);

        return CreatedAtAction(nameof(ById), new { id = activity.Id }, activity);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ActivityDto>> Update(Guid id, UpdateActivityRequest request, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(
            new UpdateActivityCommand(id, request.Title, request.MarkdownContent, request.DueDate),
            cancellationToken));

    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ActivityDto>> Publish(Guid id, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new PublishActivityCommand(id), cancellationToken));

    [HttpPost("{id:guid}/close")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ActivityDto>> Close(Guid id, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new CloseActivityCommand(id), cancellationToken));
}

public sealed record CreateActivityRequest(
    [Required][MaxLength(200)] string Title,
    [Required] string MarkdownContent,
    Guid TopicId,
    Guid? SessionId,
    DateTimeOffset? DueDate,
    SubmissionMode SubmissionMode);

public sealed record UpdateActivityRequest(
    [Required][MaxLength(200)] string Title,
    [Required] string MarkdownContent,
    DateTimeOffset? DueDate);
