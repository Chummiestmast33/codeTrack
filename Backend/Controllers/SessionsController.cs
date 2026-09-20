using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Backend.Application.Features.Sessions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/sessions")]
[Authorize]
public sealed class SessionsController : ControllerBase
{
    private readonly ISender _sender;

    public SessionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SessionDto>>> List(CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetSessionsQuery(), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SessionDto>> ById(Guid id, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetSessionByIdQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<SessionDto>> Create(CreateSessionRequest request, CancellationToken cancellationToken)
    {
        var creator = CurrentUserId();
        var session = await _sender.Send(
            new CreateSessionCommand(request.Title, request.Description, request.SessionDate, request.TopicIds, creator),
            cancellationToken);

        return CreatedAtAction(nameof(ById), new { id = session.Id }, session);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<SessionDto>> Update(Guid id, UpdateSessionRequest request, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(
            new UpdateSessionCommand(id, request.Title, request.Description, request.SessionDate, request.TopicIds),
            cancellationToken));

    [HttpPost("{id:guid}/mark-imparted")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<SessionDto>> MarkImparted(Guid id, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new MarkSessionImpartedCommand(id), cancellationToken));

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<SessionDto>> Cancel(Guid id, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new CancelSessionCommand(id), cancellationToken));

    private Guid CurrentUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}

public sealed record CreateSessionRequest(
    [Required][MaxLength(200)] string Title,
    string? Description,
    DateTimeOffset SessionDate,
    [MinLength(1)] IReadOnlyList<Guid> TopicIds);

public sealed record UpdateSessionRequest(
    [Required][MaxLength(200)] string Title,
    string? Description,
    DateTimeOffset SessionDate,
    [MinLength(1)] IReadOnlyList<Guid> TopicIds);
