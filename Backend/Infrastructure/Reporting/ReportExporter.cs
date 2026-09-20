using System.Text;
using Backend.Application.Abstractions;
using Backend.Application.Features.Reporting;
using Backend.Domain.Enums;
using Microsoft.Extensions.Options;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Backend.Infrastructure.Reporting;

/// <summary>Official exports with RN-08 header (RF-21 to RF-23). QuestPDF Community license.</summary>
public sealed class ReportExporter : IReportExporter
{
    static ReportExporter()
    {
        Settings.License = LicenseType.Community;
    }

    private readonly ReportOptions _options;

    public ReportExporter(IOptions<ReportOptions> options)
    {
        _options = options.Value;
    }

    public Task<ReportFile> ExportAttendanceAsync(AttendanceReport report, ReportFormat format, CancellationToken cancellationToken)
    {
        _options.EnsureConfigured();
        return Task.FromResult(format switch
        {
            ReportFormat.Pdf => new ReportFile(
                $"lista-asistencia-{report.GeneratedAt:yyyyMMdd}.pdf",
                "application/pdf",
                AttendancePdf(report)),
            _ => new ReportFile(
                $"lista-asistencia-{report.GeneratedAt:yyyyMMdd}.csv",
                "text/csv",
                Encoding.UTF8.GetBytes(AttendanceCsv(report)))
        });
    }

    public Task<ReportFile> ExportProgressAsync(ProgressReport report, ReportFormat format, CancellationToken cancellationToken)
    {
        _options.EnsureConfigured();
        return Task.FromResult(format switch
        {
            ReportFormat.Pdf => new ReportFile(
                $"tabla-progreso-{report.GeneratedAt:yyyyMMdd}.pdf",
                "application/pdf",
                ProgressPdf(report)),
            _ => new ReportFile(
                $"tabla-progreso-{report.GeneratedAt:yyyyMMdd}.csv",
                "text/csv",
                Encoding.UTF8.GetBytes(ProgressCsv(report)))
        });
    }

    private static string Cell(string? value)
    {
        var text = value ?? string.Empty;
        return text.IndexOfAny(['"', ',', '\n']) < 0 ? text : '"' + text.Replace("\"", "\"\"") + '"';
    }

    private string OfficialHeader()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Proyecto,{Cell(_options.ProjectName)}");
        sb.AppendLine($"Periodo,{Cell(_options.Period)}");
        sb.AppendLine($"Responsable,{Cell(_options.Responsible)}");
        sb.AppendLine($"Estudiante asesor,{Cell(_options.Advisor)}");
        return sb.ToString();
    }

    private string AttendanceCsv(AttendanceReport report)
    {
        var sb = new StringBuilder();
        sb.Append(OfficialHeader());
        sb.AppendLine("Alumno,Número de control,Presente,Retardo,Justificado,Falta,Sesiones,Asistencia %");
        foreach (var row in report.Rows)
        {
            sb.AppendLine(string.Join(',', new[]
            {
                Cell(row.FullName), Cell(row.ControlNumber),
                row.Present.ToString(), row.Late.ToString(), row.Excused.ToString(),
                row.Absent.ToString(), row.TotalSessions.ToString(),
                row.AttendancePercentage.ToString("0.0")
            }));
        }

        return sb.ToString();
    }

    private string ProgressCsv(ProgressReport report)
    {
        var sb = new StringBuilder();
        sb.Append(OfficialHeader());
        sb.Append("Alumno,Número de control");
        foreach (var topic in report.Topics)
        {
            sb.Append(',').Append(Cell(topic.Name));
        }

        sb.AppendLine();
        foreach (var row in report.Rows)
        {
            sb.Append(Cell(row.FullName)).Append(',').Append(Cell(row.ControlNumber));
            foreach (var status in row.Statuses)
            {
                sb.Append(',').Append(Cell(ProgressLabel(status)));
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string ProgressLabel(ProgressStatus status) => status switch
    {
        ProgressStatus.Completed => "Completado",
        ProgressStatus.InProgress => "En proceso",
        _ => "No iniciado"
    };

    private byte[] AttendancePdf(AttendanceReport report) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Header().Element(c => Header(c, "Lista de asistencia"));
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                    });
                    table.Header(header =>
                    {
                        header.Cell().Text("Alumno").Bold();
                        header.Cell().Text("Pres.").Bold();
                        header.Cell().Text("Ret.").Bold();
                        header.Cell().Text("Just.").Bold();
                        header.Cell().Text("%").Bold();
                    });
                    foreach (var row in report.Rows)
                    {
                        table.Cell().Text(row.FullName);
                        table.Cell().Text(row.Present.ToString());
                        table.Cell().Text(row.Late.ToString());
                        table.Cell().Text(row.Excused.ToString());
                        table.Cell().Text(row.AttendancePercentage.ToString("0.0"));
                    }
                });
                page.Footer().AlignRight().Text(t => t.Span($"Generado (UTC): {report.GeneratedAt:yyyy-MM-dd HH:mm}"));
            });
        }).GeneratePdf();

    private byte[] ProgressPdf(ProgressReport report) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Header().Element(c => Header(c, "Tabla de progreso por tema"));
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        foreach (var _ in report.Topics)
                        {
                            c.RelativeColumn(1);
                        }
                    });
                    table.Header(header =>
                    {
                        header.Cell().Text("Alumno").Bold();
                        foreach (var topic in report.Topics)
                        {
                            header.Cell().Text(topic.Name).Bold();
                        }
                    });
                    foreach (var row in report.Rows)
                    {
                        table.Cell().Text(row.FullName);
                        foreach (var status in row.Statuses)
                        {
                            table.Cell().Text(ProgressLabel(status));
                        }
                    }
                });
                page.Footer().AlignRight().Text(t => t.Span($"Generado (UTC): {report.GeneratedAt:yyyy-MM-dd HH:mm}"));
            });
        }).GeneratePdf();

    private void Header(QuestPDF.Infrastructure.IContainer container, string title)
    {
        container.Column(column =>
        {
            column.Item().Text(_options.ProjectName).Bold().FontSize(14);
            column.Item().Text($"Periodo: {_options.Period}");
            column.Item().Text($"Responsable: {_options.Responsible}");
            column.Item().Text($"Estudiante asesor: {_options.Advisor}");
            column.Item().PaddingTop(8).Text(title).Bold().FontSize(12);
        });
    }
}
