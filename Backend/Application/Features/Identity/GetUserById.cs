using Backend.Application.Abstractions;
using Backend.Application.Common;
using MediatR;

namespace Backend.Application.Features.Identity;

public sealed record GetUserByIdQuery(Guid UserId) : IRequest<UserDto>;

public sealed class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserRepository _users;

    public GetUserByIdHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        return UserDto.From(user);
    }
}
