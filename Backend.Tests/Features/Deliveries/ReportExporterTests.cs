using Backend.Application.Abstractions;
using Backend.Application.Features.Reporting;
using Backend.Domain.Enums;
using Backend.Infrastructure.Reporting;
using Microsoft.Extensions.Options;

namespace Backend.Tests.Features.Deliveries;

public sealed class ReportExporterTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 19, 2, 0, 0, TimeSpan.Zero);

    private static ReportExporter Exporter() => new(new OptionsWrapper<ReportOptions>(new ReportOptions
    {
        ProjectName = "Test Workshop",
        Period = "TEST-2026",
        Responsible = "Test Owner",
        Advisor = "Test Advisor"
    }));

    [Fact]
    public async Task Missing_Header_Config_Throws_Clear_Error_On_Export()
    {
        var exporter = new ReportExporter(new OptionsWrapper<ReportOptions>(new ReportOptions()));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            exporter.ExportAttendanceAsync(AttendanceSample(), ReportFormat.Csv, CancellationToken.None));
        Assert.Contains("ProjectName", ex.Message);
    }

    private static AttendanceReport AttendanceSample() => new(Now, new[]
    {
        new AttendanceRow("s1", "Ana, Paz", 5, 1, 0, 0, 6, 100.0),
        new AttendanceRow("s2", "Luis Rey", 3, 0, 1, 2, 6, 66.7)
    });

    private static ProgressReport ProgressSample()
    {
        var topics = new[] { new ProgressTopic(Guid.NewGuid(), "T1"), new ProgressTopic(Guid.NewGuid(), "T2") };
        return new ProgressReport(Now, topics, new[]
        {
            new ProgressRow("s1", "Ana Paz", new[] { ProgressStatus.Completed, ProgressStatus.InProgress })
        });
    }

    [Fact]
    public async Task Attendance_Csv_Has_Header_And_Escapes_Commas()
    {
        var file = await Exporter().ExportAttendanceAsync(AttendanceSample(), ReportFormat.Csv, CancellationToken.None);

        Assert.Equal("text/csv", file.ContentType);
        Assert.StartsWith("lista-asistencia-", file.FileName);
        var text = System.Text.Encoding.UTF8.GetString(file.Bytes);
        Assert.Contains("Alumno,Número de control", text);
        Assert.Contains("\"Ana, Paz\"", text);
        Assert.Contains("66.7", text);
        Assert.Contains("Test Workshop", text);
        Assert.Contains("TEST-2026", text);
        Assert.Contains("Test Owner", text);
    }

    [Fact]
    public async Task Progress_Csv_Maps_Status_Labels()
    {
        var file = await Exporter().ExportProgressAsync(ProgressSample(), ReportFormat.Csv, CancellationToken.None);

        var text = System.Text.Encoding.UTF8.GetString(file.Bytes);
        Assert.Contains(",T1,T2", text);
        Assert.Contains("Completado", text);
        Assert.Contains("En proceso", text);
    }

    [Fact]
    public async Task Pdf_Files_Start_With_Header_And_Official_Data()
    {
        var attendance = await Exporter().ExportAttendanceAsync(AttendanceSample(), ReportFormat.Pdf, CancellationToken.None);
        var progress = await Exporter().ExportProgressAsync(ProgressSample(), ReportFormat.Pdf, CancellationToken.None);

        foreach (var file in new[] { attendance, progress })
        {
            Assert.Equal("application/pdf", file.ContentType);
            Assert.StartsWith("%PDF", System.Text.Encoding.ASCII.GetString(file.Bytes, 0, 4));
            Assert.True(file.Bytes.Length > 1000);
        }
    }
}
