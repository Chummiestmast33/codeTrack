using Backend.Application.Abstractions;
using Backend.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Backend.Application.Features.Sessions;

/// <summary>Instructor creates a session linked to one or more topics (RF-06).</summary>
public sealed record CreateSessionCommand(
    string Title,
    string? Description,
    DateTimeOffset SessionDate,
    IReadOnlyList<Guid> TopicIds,
    Guid CreatedBy) : IRequest<SessionDto>;

public sealed class CreateSessionValidator : AbstractValidator<CreateSessionCommand>
{
    public CreateSessionValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TopicIds).NotEmpty().WithMessage("At least one topic is required.");
        RuleFor(x => x.CreatedBy).NotEmpty();
    }
}

public sealed class CreateSessionHandler : IRequestHandler<CreateSessionCommand, SessionDto>
{
    private readonly ISessionRepository _sessions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SessionManager _manager;
    private readonly TimeProvider _time;

    public CreateSessionHandler(
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

    public async Task<SessionDto> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        var session = Session.Create(request.Title, request.Description, request.SessionDate, request.CreatedBy, _time.GetUtcNow());
        await _sessions.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _manager.SaveTopicsAsync(session.Id, request.TopicIds, cancellationToken);
        return await _manager.LoadDtoAsync(session.Id, cancellationToken);
    }
}
