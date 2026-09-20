namespace Backend.Application.Abstractions;

public enum ReportFormat
{
    Csv,
    Pdf
}

/// <summary>Generated report file (RF-23).</summary>
public sealed record ReportFile(string FileName, string ContentType, byte[] Bytes);

public interface IReportExporter
{
    Task<ReportFile> ExportAttendanceAsync(Features.Reporting.AttendanceReport report, ReportFormat format, CancellationToken cancellationToken);

    Task<ReportFile> ExportProgressAsync(Features.Reporting.ProgressReport report, ReportFormat format, CancellationToken cancellationToken);
}
