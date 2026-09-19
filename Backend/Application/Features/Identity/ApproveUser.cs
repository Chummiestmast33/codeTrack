using Backend.Application.Abstractions;
using Backend.Application.Common;
using MediatR;

namespace Backend.Application.Features.Identity;

/// <summary>Administrator approves a pending registration (RF-04, D-01).</summary>
public sealed record ApproveUserCommand(Guid UserId) : IRequest<UserDto>;

public sealed class ApproveUserHandler : IRequestHandler<ApproveUserCommand, UserDto>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _time;

    public ApproveUserHandler(IUserRepository users, IUnitOfWork unitOfWork, TimeProvider time)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _time = time;
    }

    public async Task<UserDto> Handle(ApproveUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        user.Approve(_time.GetUtcNow());
        _users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UserDto.From(user);
    }
}
