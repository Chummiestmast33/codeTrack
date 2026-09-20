using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Backend.Application.Features.Progress;
using Backend.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Authorize]
public sealed class ProgressController : ControllerBase
{
    private readonly ISender _sender;

    public ProgressController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("api/progress/me")]
    public async Task<ActionResult<IReadOnlyList<TopicProgressDto>>> Mine(CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetStudentProgressQuery(CurrentUserId()), cancellationToken));

    [HttpGet("api/admin/progress")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<IReadOnlyList<TopicProgressDto>>> ByStudent([FromQuery] Guid userId, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetStudentProgressQuery(userId), cancellationToken));

    [HttpPost("api/admin/progress/{userId:guid}/{topicId:guid}/adjust")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<TopicProgressDto>> Adjust(Guid userId, Guid topicId, AdjustProgressRequest request, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new AdjustProgressCommand(userId, topicId, request.Status, request.Reason), cancellationToken));

    [HttpDelete("api/admin/progress/{userId:guid}/{topicId:guid}/adjust")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<TopicProgressDto>> Clear(Guid userId, Guid topicId, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new ClearProgressAdjustmentCommand(userId, topicId), cancellationToken));

    private Guid CurrentUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}

public sealed record AdjustProgressRequest(
    ProgressStatus Status,
    [Required] string Reason);
