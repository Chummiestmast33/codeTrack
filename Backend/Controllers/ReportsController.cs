using Backend.Application.Abstractions;
using Backend.Application.Features.Reporting;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Administrator")]
public sealed class ReportsController : ControllerBase
{
    private readonly ISender _sender;

    public ReportsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("attendance")]
    public async Task<IActionResult> Attendance([FromQuery] string format = "pdf", CancellationToken cancellationToken = default)
    {
        var file = await _sender.Send(new ExportAttendanceReportQuery(Parse(format)), cancellationToken);
        return File(file.Bytes, file.ContentType, file.FileName);
    }

    [HttpGet("progress")]
    public async Task<IActionResult> Progress([FromQuery] string format = "pdf", CancellationToken cancellationToken = default)
    {
        var file = await _sender.Send(new ExportProgressReportQuery(Parse(format)), cancellationToken);
        return File(file.Bytes, file.ContentType, file.FileName);
    }

    private static ReportFormat Parse(string format) =>
        format.Equals("csv", StringComparison.OrdinalIgnoreCase) ? ReportFormat.Csv : ReportFormat.Pdf;
}
