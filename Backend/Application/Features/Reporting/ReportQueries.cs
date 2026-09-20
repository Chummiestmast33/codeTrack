using Backend.Application.Abstractions;
using Backend.Application.Features.Progress;
using Backend.Domain.Enums;
using MediatR;

namespace Backend.Application.Features.Reporting;

public sealed record GetAttendanceReportQuery : IRequest<AttendanceReport>;

public sealed class GetAttendanceReportHandler : IRequestHandler<GetAttendanceReportQuery, AttendanceReport>
{
    private readonly IUserRepository _users;
    private readonly ISessionRepository _sessions;
    private readonly IAttendanceRepository _attendance;
    private readonly TimeProvider _time;

    public GetAttendanceReportHandler(
        IUserRepository users,
        ISessionRepository sessions,
        IAttendanceRepository attendance,
        TimeProvider time)
    {
        _users = users;
        _sessions = sessions;
        _attendance = attendance;
        _time = time;
    }

    public async Task<AttendanceReport> Handle(GetAttendanceReportQuery request, CancellationToken cancellationToken)
    {
        var sessions = (await _sessions.ListAsync(cancellationToken))
            .Where(s => s.Status != SessionStatus.Cancelled)
            .ToList();
        var students = (await _users.ListAsync(cancellationToken))
            .Where(u => u.Role == UserRole.Student)
            .OrderBy(u => u.FullName)
            .ToList();

        var rows = new List<AttendanceRow>(students.Count);
        foreach (var student in students)
        {
            var records = await _attendance.ListByUserAsync(student.Id, cancellationToken);
            var bySession = records.ToDictionary(a => a.SessionId);
            int present = 0, late = 0, excused = 0, absent = 0;
            foreach (var session in sessions)
            {
                if (!bySession.TryGetValue(session.Id, out var record))
                {
                    absent++;
                    continue;
                }

                switch (record.Status)
                {
                    case AttendanceStatus.Present: present++; break;
                    case AttendanceStatus.Late: late++; break;
                    case AttendanceStatus.Excused: excused++; break;
                    default: absent++; break;
                }
            }

            var total = sessions.Count;
            var percentage = total == 0 ? 0 : Math.Round((present + late + excused) * 100.0 / total, 1);
            rows.Add(new AttendanceRow(student.ControlNumber, student.FullName, present, late, excused, absent, total, percentage));
        }

        return new AttendanceReport(_time.GetUtcNow(), rows);
    }
}

public sealed record GetProgressReportQuery : IRequest<ProgressReport>;

public sealed class GetProgressReportHandler : IRequestHandler<GetProgressReportQuery, ProgressReport>
{
    private readonly IUserRepository _users;
    private readonly ITopicRepository _topics;
    private readonly ProgressEvaluator _evaluator;
    private readonly TimeProvider _time;

    public GetProgressReportHandler(
        IUserRepository users,
        ITopicRepository topics,
        ProgressEvaluator evaluator,
        TimeProvider time)
    {
        _users = users;
        _topics = topics;
        _evaluator = evaluator;
        _time = time;
    }

    public async Task<ProgressReport> Handle(GetProgressReportQuery request, CancellationToken cancellationToken)
    {
        var topics = (await _topics.ListOrderedAsync(cancellationToken)).Where(t => t.IsActive).ToList();
        var students = (await _users.ListAsync(cancellationToken))
            .Where(u => u.Role == UserRole.Student)
            .OrderBy(u => u.FullName)
            .ToList();

        var rows = new List<ProgressRow>(students.Count);
        foreach (var student in students)
        {
            var progress = await _evaluator.EvaluateAsync(student.Id, cancellationToken);
            var byTopic = progress.ToDictionary(p => p.TopicId);
            rows.Add(new ProgressRow(
                student.ControlNumber,
                student.FullName,
                topics.Select(t => byTopic.TryGetValue(t.Id, out var p) ? p.EffectiveStatus : ProgressStatus.NotStarted).ToList()));
        }

        return new ProgressReport(
            _time.GetUtcNow(),
            topics.Select(t => new ProgressTopic(t.Id, t.Name)).ToList(),
            rows);
    }
}

public sealed record ExportAttendanceReportQuery(ReportFormat Format) : IRequest<ReportFile>;

public sealed class ExportAttendanceReportHandler : IRequestHandler<ExportAttendanceReportQuery, ReportFile>
{
    private readonly ISender _sender;
    private readonly IReportExporter _exporter;

    public ExportAttendanceReportHandler(ISender sender, IReportExporter exporter)
    {
        _sender = sender;
        _exporter = exporter;
    }

    public async Task<ReportFile> Handle(ExportAttendanceReportQuery request, CancellationToken cancellationToken)
    {
        var report = await _sender.Send(new GetAttendanceReportQuery(), cancellationToken);
        return await _exporter.ExportAttendanceAsync(report, request.Format, cancellationToken);
    }
}

public sealed record ExportProgressReportQuery(ReportFormat Format) : IRequest<ReportFile>;

public sealed class ExportProgressReportHandler : IRequestHandler<ExportProgressReportQuery, ReportFile>
{
    private readonly ISender _sender;
    private readonly IReportExporter _exporter;

    public ExportProgressReportHandler(ISender sender, IReportExporter exporter)
    {
        _sender = sender;
        _exporter = exporter;
    }

    public async Task<ReportFile> Handle(ExportProgressReportQuery request, CancellationToken cancellationToken)
    {
        var report = await _sender.Send(new GetProgressReportQuery(), cancellationToken);
        return await _exporter.ExportProgressAsync(report, request.Format, cancellationToken);
    }
}
