using Backend.Application.Abstractions;
using Backend.Application.Common;
using Backend.Domain.Entities;
using Backend.Domain.Rules;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

namespace Backend.Application.Features.Identity;

/// <summary>Student self-registration (RF-01, D-01). The account starts pending.</summary>
public sealed record RegisterStudentCommand(
    string ControlNumber,
    string FullName,
    string Email,
    string Password) : IRequest<UserDto>;

public sealed class RegisterStudentValidator : AbstractValidator<RegisterStudentCommand>
{
    public RegisterStudentValidator(IOptions<AuthOptions> options)
    {
        var allowedDomain = options.Value.AllowedEmailDomain?.Trim().TrimStart('@');

        RuleFor(x => x.ControlNumber)
            .Must(c => !string.IsNullOrWhiteSpace(c))
            .WithMessage("Control number is required.");

        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);

        // D-02: the allowed domain is enforced only when configured.
        When(_ => !string.IsNullOrEmpty(allowedDomain), () =>
        {
            RuleFor(x => x.Email)
                .Must(email => email.Trim().EndsWith("@" + allowedDomain, StringComparison.OrdinalIgnoreCase))
                .WithMessage($"Email must belong to the '{allowedDomain}' domain.");
        });

        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

public sealed class RegisterStudentHandler : IRequestHandler<RegisterStudentCommand, UserDto>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly TimeProvider _time;

    public RegisterStudentHandler(
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

    public async Task<UserDto> Handle(RegisterStudentCommand request, CancellationToken cancellationToken)
    {
        var controlNumber = ControlNumberRules.Normalize(request.ControlNumber);
        var email = request.Email.Trim();

        if (await _users.GetByControlNumberAsync(controlNumber, cancellationToken) is not null)
        {
            throw new ConflictException($"Control number '{controlNumber}' is already registered.");
        }

        var user = User.RegisterStudent(
            controlNumber,
            request.FullName,
            email,
            _passwordHasher.Hash(request.Password),
            _time.GetUtcNow());

        await _users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UserDto.From(user);
    }
}
