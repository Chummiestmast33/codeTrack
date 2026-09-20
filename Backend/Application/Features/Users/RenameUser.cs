using Backend.Application.Abstractions;
using Backend.Application.Common;
using Backend.Application.Features.Identity;
using FluentValidation;
using MediatR;

namespace Backend.Application.Features.Users;

/// <summary>Administrator edits a user's full name (RF-04).</summary>
public sealed record RenameUserCommand(Guid UserId, string FullName) : IRequest<UserDto>;

public sealed class RenameUserValidator : AbstractValidator<RenameUserCommand>
{
    public RenameUserValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
    }
}

public sealed class RenameUserHandler : IRequestHandler<RenameUserCommand, UserDto>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _time;

    public RenameUserHandler(IUserRepository users, IUnitOfWork unitOfWork, TimeProvider time)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _time = time;
    }

    public async Task<UserDto> Handle(RenameUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        user.Rename(request.FullName, _time.GetUtcNow());
        _users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UserDto.From(user);
    }
}
