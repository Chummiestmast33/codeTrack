using System.ComponentModel.DataAnnotations;
using Backend.Application.Features.Topics;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/topics")]
[Authorize]
public sealed class TopicsController : ControllerBase
{
    private readonly ISender _sender;

    public TopicsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TopicDto>>> List(CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetTopicsQuery(), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TopicDto>> ById(Guid id, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetTopicByIdQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<TopicDto>> Create(CreateTopicRequest request, CancellationToken cancellationToken)
    {
        var topic = await _sender.Send(
            new CreateTopicCommand(request.Name, request.Description, request.OrderNumber),
            cancellationToken);

        return CreatedAtAction(nameof(ById), new { id = topic.Id }, topic);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<TopicDto>> Update(Guid id, UpdateTopicRequest request, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(
            new UpdateTopicCommand(id, request.Name, request.Description, request.OrderNumber),
            cancellationToken));

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<TopicDto>> Deactivate(Guid id, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new DeactivateTopicCommand(id), cancellationToken));

    [HttpPost("{id:guid}/activate")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<TopicDto>> Activate(Guid id, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new ActivateTopicCommand(id), cancellationToken));
}

public sealed record CreateTopicRequest(
    [Required][MaxLength(200)] string Name,
    string? Description,
    int OrderNumber);

public sealed record UpdateTopicRequest(
    [Required][MaxLength(200)] string Name,
    string? Description,
    int OrderNumber);
