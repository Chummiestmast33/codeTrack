using Backend.Application.Abstractions;
using Backend.Application.Common;
using FluentValidation;
using MediatR;

namespace Backend.Application.Features.Identity;

/// <summary>Administrator resets a password; no self-service recovery (RF-04, D-02).</summary>
public sealed record ResetPasswordCommand(Guid UserId, string NewPassword) : IRequest<UserDto>;

public sealed class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8);
    }
}

public sealed class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, UserDto>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly TimeProvider _time;

    public ResetPasswordHandler(
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        TimeProvider time)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _time = time;
    }

    public async Task<UserDto> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        user.SetPasswordHash(_passwordHasher.Hash(request.NewPassword), _time.GetUtcNow());
        _users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UserDto.From(user);
    }
}
