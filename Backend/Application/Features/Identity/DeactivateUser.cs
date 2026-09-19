using Backend.Application.Abstractions;
using Backend.Application.Common;
using MediatR;

namespace Backend.Application.Features.Identity;

public sealed record DeactivateUserCommand(Guid UserId) : IRequest<UserDto>;

public sealed class DeactivateUserHandler : IRequestHandler<DeactivateUserCommand, UserDto>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _time;

    public DeactivateUserHandler(IUserRepository users, IUnitOfWork unitOfWork, TimeProvider time)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _time = time;
    }

    public async Task<UserDto> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        user.Deactivate(_time.GetUtcNow());
        _users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UserDto.From(user);
    }
}
