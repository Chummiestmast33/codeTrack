using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Backend.Application.Abstractions;
using Backend.Application.Features.Attendance;
using Backend.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Backend.Controllers;

[ApiController]
[Authorize]
public sealed class AttendanceController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IQrCodeGenerator _qrImages;
    private readonly QrOptions _qrOptions;

    public AttendanceController(ISender sender, IQrCodeGenerator qrImages, IOptions<QrOptions> qrOptions)
    {
        _sender = sender;
        _qrImages = qrImages;
        _qrOptions = qrOptions.Value;
    }

    [HttpGet("api/sessions/{sessionId:guid}/attendance")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<IReadOnlyList<AttendanceDto>>> BySession(Guid sessionId, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetAttendanceBySessionQuery(sessionId), cancellationToken));

    [HttpPost("api/sessions/{sessionId:guid}/attendance/manual")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<AttendanceDto>> RegisterManual(
        Guid sessionId, ManualAttendanceRequest request, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(
            new RegisterManualAttendanceCommand(sessionId, request.UserId, request.Status, request.Observation, CurrentUserId()),
            cancellationToken));

    [HttpPatch("api/attendance/{id:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<AttendanceDto>> Update(Guid id, UpdateAttendanceRequest request, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(
            new UpdateAttendanceCommand(id, request.Status, request.Observation),
            cancellationToken));

    [HttpPost("api/attendance/qr/{token}")]
    public async Task<ActionResult<AttendanceDto>> RegisterByQr(string token, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new RegisterAttendanceByQrCommand(token, CurrentUserId()), cancellationToken));

    [HttpGet("api/admin/sessions/{sessionId:guid}/qr")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<QrTicketDto>> GetQr(Guid sessionId, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetQrQuery(sessionId), cancellationToken));

    [HttpPost("api/admin/sessions/{sessionId:guid}/qr/regenerate")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<QrTicketDto>> RegenerateQr(Guid sessionId, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GenerateQrTokenCommand(sessionId), cancellationToken));

    [HttpGet("api/admin/sessions/{sessionId:guid}/qr/image")]
    [Authorize(Roles = "Administrator")]
    [Produces("image/png")]
    public async Task<IActionResult> QrImage(Guid sessionId, CancellationToken cancellationToken)
    {
        var ticket = await _sender.Send(new GetQrQuery(sessionId), cancellationToken);
        var png = _qrImages.GeneratePng(ticket.AttendUrl, _qrOptions.ImageSizePixels);
        return File(png, "image/png", $"qr-{sessionId:N}.png");
    }

    private Guid CurrentUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}

public sealed record ManualAttendanceRequest(
    Guid UserId,
    AttendanceStatus Status,
    string? Observation);

public sealed record UpdateAttendanceRequest(
    AttendanceStatus Status,
    string? Observation);
