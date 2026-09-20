using Backend.Application.Abstractions;
using Backend.Application.Common;
using FluentValidation;
using MediatR;

namespace Backend.Application.Features.Sessions;

public sealed record UpdateSessionCommand(
    Guid SessionId,
    string Title,
    string? Description,
    DateTimeOffset SessionDate,
    IReadOnlyList<Guid> TopicIds) : IRequest<SessionDto>;

public sealed class UpdateSessionValidator : AbstractValidator<UpdateSessionCommand>
{
    public UpdateSessionValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TopicIds).NotEmpty().WithMessage("At least one topic is required.");
    }
}

public sealed class UpdateSessionHandler : IRequestHandler<UpdateSessionCommand, SessionDto>
{
    private readonly ISessionRepository _sessions;
    private readonly SessionManager _manager;
    private readonly TimeProvider _time;

    public UpdateSessionHandler(ISessionRepository sessions, SessionManager manager, TimeProvider time)
    {
        _sessions = sessions;
        _manager = manager;
        _time = time;
    }

    public async Task<SessionDto> Handle(UpdateSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessions.GetByIdAsync(request.SessionId, cancellationToken)
            ?? throw new NotFoundException("Session", request.SessionId);

        session.UpdateDetails(request.Title, request.Description, request.SessionDate, _time.GetUtcNow());
        _sessions.Update(session);
        await _manager.SaveTopicsAsync(session.Id, request.TopicIds, cancellationToken);
        return await _manager.LoadDtoAsync(session.Id, cancellationToken);
    }
}
