using Backend.Application.Abstractions;
using Backend.Application.Common;
using MediatR;

namespace Backend.Application.Features.Attendance;

public sealed record GetAttendanceBySessionQuery(Guid SessionId) : IRequest<IReadOnlyList<AttendanceDto>>;

public sealed class GetAttendanceBySessionHandler : IRequestHandler<GetAttendanceBySessionQuery, IReadOnlyList<AttendanceDto>>
{
    private readonly IAttendanceRepository _attendance;
    private readonly ISessionRepository _sessions;
    private readonly IUserRepository _users;
    private readonly AttendanceEnricher _enricher;

    public GetAttendanceBySessionHandler(
        IAttendanceRepository attendance,
        ISessionRepository sessions,
        IUserRepository users,
        AttendanceEnricher enricher)
    {
        _attendance = attendance;
        _sessions = sessions;
        _users = users;
        _enricher = enricher;
    }

    public async Task<IReadOnlyList<AttendanceDto>> Handle(GetAttendanceBySessionQuery request, CancellationToken cancellationToken)
    {
        if (await _sessions.GetByIdAsync(request.SessionId, cancellationToken) is null)
        {
            throw new NotFoundException("Session", request.SessionId);
        }

        var records = await _attendance.ListBySessionAsync(request.SessionId, cancellationToken);
        var result = new List<AttendanceDto>(records.Count);
        foreach (var record in records)
        {
            var user = await _users.GetByIdAsync(record.UserId, cancellationToken)
                ?? throw new NotFoundException("User", record.UserId);
            result.Add(_enricher.ToDto(record, user));
        }

        return result;
    }
}
