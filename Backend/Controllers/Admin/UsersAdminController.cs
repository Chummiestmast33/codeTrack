using Backend.Application.Features.Identity;
using Backend.Application.Features.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Administrator")]
public sealed class UsersAdminController : ControllerBase
{
    private readonly ISender _sender;

    public UsersAdminController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> List(CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetUsersQuery(), cancellationToken));

    [HttpGet("pending")]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> Pending(CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetPendingRegistrationsQuery(), cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> ById(Guid id, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetUserByIdQuery(id), cancellationToken));

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<UserDto>> Approve(Guid id, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new ApproveUserCommand(id), cancellationToken));

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<UserDto>> Reject(Guid id, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new RejectUserCommand(id), cancellationToken));

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<UserDto>> Activate(Guid id, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new ActivateUserCommand(id), cancellationToken));

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<UserDto>> Deactivate(Guid id, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new DeactivateUserCommand(id), cancellationToken));

    [HttpPost("{id:guid}/reset-password")]
    public async Task<ActionResult<UserDto>> ResetPassword(Guid id, ResetPasswordRequest request, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new ResetPasswordCommand(id, request.NewPassword), cancellationToken));

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<UserDto>> Rename(Guid id, RenameUserRequest request, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new RenameUserCommand(id, request.FullName), cancellationToken));
}
