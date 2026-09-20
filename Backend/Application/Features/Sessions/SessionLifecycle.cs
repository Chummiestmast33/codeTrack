using Backend.Application.Abstractions;
using Backend.Application.Common;
using MediatR;

namespace Backend.Application.Features.Sessions;

public sealed record MarkSessionImpartedCommand(Guid SessionId) : IRequest<SessionDto>;

public sealed class MarkSessionImpartedHandler : IRequestHandler<MarkSessionImpartedCommand, SessionDto>
{
    private readonly ISessionRepository _sessions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SessionManager _manager;
    private readonly TimeProvider _time;

    public MarkSessionImpartedHandler(
        ISessionRepository sessions,
        IUnitOfWork unitOfWork,
        SessionManager manager,
        TimeProvider time)
    {
        _sessions = sessions;
        _unitOfWork = unitOfWork;
        _manager = manager;
        _time = time;
    }

    public async Task<SessionDto> Handle(MarkSessionImpartedCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessions.GetByIdAsync(request.SessionId, cancellationToken)
            ?? throw new NotFoundException("Session", request.SessionId);

        session.MarkImparted(_time.GetUtcNow());
        _sessions.Update(session);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await _manager.LoadDtoAsync(session.Id, cancellationToken);
    }
}

public sealed record CancelSessionCommand(Guid SessionId) : IRequest<SessionDto>;

public sealed class CancelSessionHandler : IRequestHandler<CancelSessionCommand, SessionDto>
{
    private readonly ISessionRepository _sessions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SessionManager _manager;
    private readonly TimeProvider _time;

    public CancelSessionHandler(
        ISessionRepository sessions,
        IUnitOfWork unitOfWork,
        SessionManager manager,
        TimeProvider time)
    {
        _sessions = sessions;
        _unitOfWork = unitOfWork;
        _manager = manager;
        _time = time;
    }

    public async Task<SessionDto> Handle(CancelSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessions.GetByIdAsync(request.SessionId, cancellationToken)
            ?? throw new NotFoundException("Session", request.SessionId);

        session.Cancel(_time.GetUtcNow());
        _sessions.Update(session);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await _manager.LoadDtoAsync(session.Id, cancellationToken);
    }
}
