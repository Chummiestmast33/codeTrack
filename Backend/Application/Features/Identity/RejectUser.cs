using Backend.Application.Abstractions;
using Backend.Application.Common;
using MediatR;

namespace Backend.Application.Features.Identity;

/// <summary>Administrator rejects a pending registration (RF-04, D-01).</summary>
public sealed record RejectUserCommand(Guid UserId) : IRequest<UserDto>;

public sealed class RejectUserHandler : IRequestHandler<RejectUserCommand, UserDto>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _time;

    public RejectUserHandler(IUserRepository users, IUnitOfWork unitOfWork, TimeProvider time)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _time = time;
    }

    public async Task<UserDto> Handle(RejectUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        user.Reject(_time.GetUtcNow());
        _users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UserDto.From(user);
    }
}
