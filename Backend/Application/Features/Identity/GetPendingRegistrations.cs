using Backend.Application.Abstractions;
using MediatR;

namespace Backend.Application.Features.Identity;

/// <summary>Administrator reviews pending registrations (RF-04, D-01).</summary>
public sealed record GetPendingRegistrationsQuery : IRequest<IReadOnlyList<UserDto>>;

public sealed class GetPendingRegistrationsHandler : IRequestHandler<GetPendingRegistrationsQuery, IReadOnlyList<UserDto>>
{
    private readonly IUserRepository _users;

    public GetPendingRegistrationsHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<IReadOnlyList<UserDto>> Handle(GetPendingRegistrationsQuery request, CancellationToken cancellationToken)
    {
        var users = await _users.ListPendingAsync(cancellationToken);
        return users.Select(UserDto.From).ToList();
    }
}
