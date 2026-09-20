using Backend.Application.Abstractions;
using Backend.Application.Common;
using Backend.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Backend.Application.Features.Attendance;

public sealed record UpdateAttendanceCommand(Guid AttendanceId, AttendanceStatus Status, string? Observation) : IRequest<AttendanceDto>;

public sealed class UpdateAttendanceValidator : AbstractValidator<UpdateAttendanceCommand>
{
    public UpdateAttendanceValidator()
    {
        RuleFor(x => x.AttendanceId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}

public sealed class UpdateAttendanceHandler : IRequestHandler<UpdateAttendanceCommand, AttendanceDto>
{
    private readonly IAttendanceRepository _attendance;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AttendanceEnricher _enricher;
    private readonly TimeProvider _time;

    public UpdateAttendanceHandler(
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

    public async Task<AttendanceDto> Handle(UpdateAttendanceCommand request, CancellationToken cancellationToken)
    {
        var record = await _attendance.GetByIdAsync(request.AttendanceId, cancellationToken)
            ?? throw new NotFoundException("Attendance", request.AttendanceId);

        var user = await _users.GetByIdAsync(record.UserId, cancellationToken)
            ?? throw new NotFoundException("User", record.UserId);

        record.UpdateStatus(request.Status, request.Observation, _time.GetUtcNow());
        _attendance.Update(record);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _enricher.ToDto(record, user);
    }
}
