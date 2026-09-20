using Backend.Application.Abstractions;
using Backend.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Backend.Application.Features.Attendance;

/// <summary>Builds QR tickets: expiry and attend URLs (RN-09).</summary>
public sealed class QrTicketBuilder
{
    private readonly QrOptions _options;

    public QrTicketBuilder(IOptions<QrOptions> options)
    {
        _options = options.Value;
    }

    public DateTimeOffset ExpiresAt(DateTimeOffset now) => now.AddMinutes(_options.ExpiryMinutes);

    public QrTicketDto Build(QrToken qr) =>
        new(qr.Token, $"{_options.FrontendBaseUrl.TrimEnd('/')}/app/attend/{qr.Token}", qr.ExpiresAt);
}

/// <summary>Enriches attendance records with student data for display.</summary>
public sealed class AttendanceEnricher
{
    public AttendanceDto ToDto(AttendanceRecord record, User user) =>
        AttendanceDto.From(record, user);
}
