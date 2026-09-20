using Backend.Application.Abstractions;
using Backend.Application.Common;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Domain.Rules;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

namespace Backend.Application.Features.Attendance;

/// <summary>Authenticated student registers attendance via QR (RF-10, D-04).</summary>
public sealed record RegisterAttendanceByQrCommand(string Token, Guid UserId) : IRequest<AttendanceDto>;

public sealed class RegisterAttendanceByQrValidator : AbstractValidator<RegisterAttendanceByQrCommand>
{
    public RegisterAttendanceByQrValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public sealed class RegisterAttendanceByQrHandler : IRequestHandler<RegisterAttendanceByQrCommand, AttendanceDto>
{
    private readonly IQrTokenRepository _qrTokens;
    private readonly ISessionRepository _sessions;
    private readonly IAttendanceRepository _attendance;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AttendanceEnricher _enricher;
    private readonly TimeProvider _time;

    public RegisterAttendanceByQrHandler(
        IQrTokenRepository qrTokens,
        ISessionRepository sessions,
        IAttendanceRepository attendance,
        IUserRepository users,
        IUnitOfWork unitOfWork,
        AttendanceEnricher enricher,
        TimeProvider time)
    {
        _qrTokens = qrTokens;
        _sessions = sessions;
        _attendance = attendance;
        _users = users;
        _unitOfWork = unitOfWork;
        _enricher = enricher;
        _time = time;
    }

    public async Task<AttendanceDto> Handle(RegisterAttendanceByQrCommand request, CancellationToken cancellationToken)
    {
        var now = _time.GetUtcNow();

        var qr = await _qrTokens.GetByTokenAsync(request.Token.Trim(), cancellationToken)
            ?? throw new NotFoundException("QrToken", request.Token);

        if (qr.IsExpired(now))
        {
            throw new GoneException("The QR code has expired.");
        }

        var session = await _sessions.GetByIdAsync(qr.SessionId, cancellationToken)
            ?? throw new NotFoundException("Session", qr.SessionId);

        if (session.Status != SessionStatus.Planned)
        {
            throw new ConflictException("The session is not open for attendance.");
        }

        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        if (!AttendancePolicy.CanRegisterViaQr(user))
        {
            throw new ForbiddenException("Only approved and active accounts can register attendance.");
        }

        if (await _attendance.GetAsync(session.Id, user.Id, cancellationToken) is not null)
        {
            throw new ConflictException("Attendance is already registered for this student and session.");
        }

        var record = AttendanceRecord.Register(session.Id, user.Id, AttendanceStatus.Present, user.Id, now);
        await _attendance.AddAsync(record, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _enricher.ToDto(record, user);
    }
}

/// <summary>Instructor (re)generates the QR token for a session (RF-08).</summary>
public sealed record GenerateQrTokenCommand(Guid SessionId) : IRequest<QrTicketDto>;

public sealed class GenerateQrTokenHandler : IRequestHandler<GenerateQrTokenCommand, QrTicketDto>
{
    private readonly IQrTokenRepository _qrTokens;
    private readonly ISessionRepository _sessions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly QrTicketBuilder _tickets;
    private readonly TimeProvider _time;

    public GenerateQrTokenHandler(
        IQrTokenRepository qrTokens,
        ISessionRepository sessions,
        IUnitOfWork unitOfWork,
        QrTicketBuilder tickets,
        TimeProvider time)
    {
        _qrTokens = qrTokens;
        _sessions = sessions;
        _unitOfWork = unitOfWork;
        _tickets = tickets;
        _time = time;
    }

    public async Task<QrTicketDto> Handle(GenerateQrTokenCommand request, CancellationToken cancellationToken)
    {
        if (await _sessions.GetByIdAsync(request.SessionId, cancellationToken) is null)
        {
            throw new NotFoundException("Session", request.SessionId);
        }

        var qr = QrToken.Create(request.SessionId, QrToken.GenerateToken(), _tickets.ExpiresAt(_time.GetUtcNow()), _time.GetUtcNow());
        await _qrTokens.AddAsync(qr, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _tickets.Build(qr);
    }
}

public sealed record GetQrQuery(Guid SessionId) : IRequest<QrTicketDto>;

public sealed class GetQrHandler : IRequestHandler<GetQrQuery, QrTicketDto>
{
    private readonly IQrTokenRepository _qrTokens;
    private readonly QrTicketBuilder _tickets;
    private readonly TimeProvider _time;

    public GetQrHandler(IQrTokenRepository qrTokens, QrTicketBuilder tickets, TimeProvider time)
    {
        _qrTokens = qrTokens;
        _tickets = tickets;
        _time = time;
    }

    public async Task<QrTicketDto> Handle(GetQrQuery request, CancellationToken cancellationToken)
    {
        var now = _time.GetUtcNow();
        var qr = await _qrTokens.GetLatestBySessionAsync(request.SessionId, cancellationToken);
        if (qr is null || qr.IsExpired(now))
        {
            throw new GoneException("There is no valid QR code for this session.");
        }

        return _tickets.Build(qr);
    }
}
