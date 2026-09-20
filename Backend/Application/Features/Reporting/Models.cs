using Backend.Domain.Enums;

namespace Backend.Application.Features.Reporting;

/// <summary>Official attendance list (RF-21). Attended = Present, Late or Excused.</summary>
public sealed record AttendanceRow(
    string ControlNumber,
    string FullName,
    int Present,
    int Late,
    int Excused,
    int Absent,
    int TotalSessions,
    double AttendancePercentage);

public sealed record AttendanceReport(
    DateTimeOffset GeneratedAt,
    IReadOnlyList<AttendanceRow> Rows);

/// <summary>Progress table per official topic (RF-22).</summary>
public sealed record ProgressTopic(Guid Id, string Name);

public sealed record ProgressRow(
    string ControlNumber,
    string FullName,
    IReadOnlyList<ProgressStatus> Statuses);

public sealed record ProgressReport(
    DateTimeOffset GeneratedAt,
    IReadOnlyList<ProgressTopic> Topics,
    IReadOnlyList<ProgressRow> Rows);
