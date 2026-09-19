using Backend.Application.Abstractions;
using MediatR;

namespace Backend.Application.Features.Identity;

/// <summary>Administrator lists users (RF-04).</summary>
public sealed record GetUsersQuery : IRequest<IReadOnlyList<UserDto>>;

public sealed class GetUsersHandler : IRequestHandler<GetUsersQuery, IReadOnlyList<UserDto>>
{
    private readonly IUserRepository _users;

    public GetUsersHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<IReadOnlyList<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _users.ListAsync(cancellationToken);
        return users.Select(UserDto.From).ToList();
    }
}
