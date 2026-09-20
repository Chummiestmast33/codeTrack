using Backend.Application.Abstractions;
using Backend.Application.Common;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Backend.Application.Features.Attendance;

/// <summary>Instructor registers attendance manually (RF-07, RF-11).</summary>
public sealed record RegisterManualAttendanceCommand(
    Guid SessionId,
    Guid UserId,
    AttendanceStatus Status,
    string? Observation,
    Guid RegisteredBy) : IRequest<AttendanceDto>;

public sealed class RegisterManualAttendanceValidator : AbstractValidator<RegisterManualAttendanceCommand>
{
    public RegisterManualAttendanceValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RegisteredBy).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}

public sealed class RegisterManualAttendanceHandler : IRequestHandler<RegisterManualAttendanceCommand, AttendanceDto>
{
    private readonly IAttendanceRepository _attendance;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AttendanceEnricher _enricher;
    private readonly TimeProvider _time;

    public RegisterManualAttendanceHandler(
        IAttendanceRepository attendance,
        IUserRepository users,
        IUnitOfWork unitOfWork,
        AttendanceEnricher enricher,
        TimeProvider time)
    {
        _attendance = attendance;
        _users = users;
        _unitOfWork = unitOfWork;
        _enricher = enricher;
        _time = time;
    }

    public async Task<AttendanceDto> Handle(RegisterManualAttendanceCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        if (await _attendance.GetAsync(request.SessionId, request.UserId, cancellationToken) is not null)
        {
            throw new ConflictException("Attendance is already registered for this student and session.");
        }

        var record = AttendanceRecord.Register(
            request.SessionId, request.UserId, request.Status, request.RegisteredBy,
            _time.GetUtcNow(), request.Observation);

        await _attendance.AddAsync(record, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _enricher.ToDto(record, user);
    }
}
