using Backend.Application.Abstractions;
using Backend.Application.Common;
using MediatR;

namespace Backend.Application.Features.Identity;

/// <summary>Administrator activates or deactivates an account (RF-04).</summary>
public sealed record ActivateUserCommand(Guid UserId) : IRequest<UserDto>;

public sealed class ActivateUserHandler : IRequestHandler<ActivateUserCommand, UserDto>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _time;

    public ActivateUserHandler(IUserRepository users, IUnitOfWork unitOfWork, TimeProvider time)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _time = time;
    }

    public async Task<UserDto> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        user.Activate(_time.GetUtcNow());
        _users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UserDto.From(user);
    }
}
