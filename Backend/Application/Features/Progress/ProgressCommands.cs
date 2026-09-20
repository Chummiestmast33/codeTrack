using Backend.Application.Abstractions;
using Backend.Application.Common;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Backend.Application.Features.Progress;

public sealed record GetStudentProgressQuery(Guid UserId) : IRequest<IReadOnlyList<TopicProgressDto>>;

public sealed class GetStudentProgressHandler : IRequestHandler<GetStudentProgressQuery, IReadOnlyList<TopicProgressDto>>
{
    private readonly IUserRepository _users;
    private readonly ProgressEvaluator _evaluator;

    public GetStudentProgressHandler(IUserRepository users, ProgressEvaluator evaluator)
    {
        _users = users;
        _evaluator = evaluator;
    }

    public async Task<IReadOnlyList<TopicProgressDto>> Handle(GetStudentProgressQuery request, CancellationToken cancellationToken)
    {
        if (await _users.GetByIdAsync(request.UserId, cancellationToken) is null)
        {
            throw new NotFoundException("User", request.UserId);
        }

        return await _evaluator.EvaluateAsync(request.UserId, cancellationToken);
    }
}

public sealed record AdjustProgressCommand(Guid UserId, Guid TopicId, ProgressStatus Status, string Reason) : IRequest<TopicProgressDto>;

public sealed class AdjustProgressValidator : AbstractValidator<AdjustProgressCommand>
{
    public AdjustProgressValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.TopicId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Reason).NotEmpty();
    }
}

public sealed class AdjustProgressHandler : IRequestHandler<AdjustProgressCommand, TopicProgressDto>
{
    private readonly IProgressRepository _progress;
    private readonly ProgressEvaluator _evaluator;
    private readonly TimeProvider _time;

    public AdjustProgressHandler(IProgressRepository progress, ProgressEvaluator evaluator, TimeProvider time)
    {
        _progress = progress;
        _evaluator = evaluator;
        _time = time;
    }

    public async Task<TopicProgressDto> Handle(AdjustProgressCommand request, CancellationToken cancellationToken)
    {
        var now = _time.GetUtcNow();
        var existing = await _progress.GetAsync(request.UserId, request.TopicId, cancellationToken);
        var record = existing ?? ProgressRecord.Create(request.UserId, request.TopicId, now);

        record.AdjustManually(request.Status, request.Reason, now);
        if (existing is null)
        {
            await _progress.AddAsync(record, cancellationToken);
        }
        else
        {
            _progress.Update(record);
        }

        var progress = await _evaluator.EvaluateAsync(request.UserId, cancellationToken);
        return progress.First(p => p.TopicId == request.TopicId);
    }
}

public sealed record ClearProgressAdjustmentCommand(Guid UserId, Guid TopicId) : IRequest<TopicProgressDto>;

public sealed class ClearProgressAdjustmentHandler : IRequestHandler<ClearProgressAdjustmentCommand, TopicProgressDto>
{
    private readonly IProgressRepository _progress;
    private readonly ProgressEvaluator _evaluator;
    private readonly TimeProvider _time;

    public ClearProgressAdjustmentHandler(IProgressRepository progress, ProgressEvaluator evaluator, TimeProvider time)
    {
        _progress = progress;
        _evaluator = evaluator;
        _time = time;
    }

    public async Task<TopicProgressDto> Handle(ClearProgressAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var record = await _progress.GetAsync(request.UserId, request.TopicId, cancellationToken)
            ?? throw new NotFoundException("Progress", $"{request.UserId}/{request.TopicId}");

        record.ClearAdjustment(_time.GetUtcNow());
        _progress.Update(record);

        var progress = await _evaluator.EvaluateAsync(request.UserId, cancellationToken);
        return progress.First(p => p.TopicId == request.TopicId);
    }
}
