using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.Features.Attendance;

public sealed record AttendanceDto(
    Guid Id,
    Guid SessionId,
    Guid UserId,
    string ControlNumber,
    string FullName,
    AttendanceStatus Status,
    string? Observation,
    DateTimeOffset CreatedAt)
{
    public static AttendanceDto From(AttendanceRecord record, User user) => new(
        record.Id, record.SessionId, record.UserId,
        user.ControlNumber, user.FullName,
        record.Status, record.Observation, record.CreatedAt);
}

public sealed record QrTicketDto(string Token, string AttendUrl, DateTimeOffset ExpiresAt);
