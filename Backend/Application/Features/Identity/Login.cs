using Backend.Application.Abstractions;
using Backend.Application.Common;
using Backend.Domain.Rules;
using FluentValidation;
using MediatR;

namespace Backend.Application.Features.Identity;

/// <summary>Sign-in with control number and password (RF-02, D-01).</summary>
public sealed record LoginCommand(string ControlNumber, string Password) : IRequest<AuthResultDto>;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.ControlNumber)
            .Must(c => !string.IsNullOrWhiteSpace(c))
            .WithMessage("Control number is required.");
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserTokenService _tokens;

    public LoginHandler(IUserRepository users, IPasswordHasher passwordHasher, IUserTokenService tokens)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokens = tokens;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByControlNumberAsync(
            ControlNumberRules.Normalize(request.ControlNumber), cancellationToken);

        if (user is null || !_passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            throw new UnauthorizedException();
        }

        if (!user.CanSignIn)
        {
            throw new ForbiddenException("Account is pending approval, rejected, or deactivated.");
        }

        var token = _tokens.GenerateToken(user);
        return new AuthResultDto(token.Token, token.ExpiresAt, UserDto.From(user));
    }
}
