using Backend.Application.Abstractions;
using Backend.Application.Common;
using MediatR;

namespace Backend.Application.Features.Sessions;

public sealed record GetSessionsQuery : IRequest<IReadOnlyList<SessionDto>>;

public sealed class GetSessionsHandler : IRequestHandler<GetSessionsQuery, IReadOnlyList<SessionDto>>
{
    private readonly ISessionRepository _sessions;
    private readonly SessionManager _manager;

    public GetSessionsHandler(ISessionRepository sessions, SessionManager manager)
    {
        _sessions = sessions;
        _manager = manager;
    }

    public async Task<IReadOnlyList<SessionDto>> Handle(GetSessionsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _sessions.ListAsync(cancellationToken);
        var result = new List<SessionDto>(sessions.Count);
        foreach (var session in sessions)
        {
            result.Add(await _manager.LoadDtoAsync(session.Id, cancellationToken));
        }

        return result;
    }
}

public sealed record GetSessionByIdQuery(Guid SessionId) : IRequest<SessionDto>;

public sealed class GetSessionByIdHandler : IRequestHandler<GetSessionByIdQuery, SessionDto>
{
    private readonly SessionManager _manager;

    public GetSessionByIdHandler(SessionManager manager)
    {
        _manager = manager;
    }

    public async Task<SessionDto> Handle(GetSessionByIdQuery request, CancellationToken cancellationToken) =>
        await _manager.LoadDtoAsync(request.SessionId, cancellationToken);
}
