using Backend.Application.Abstractions;
using Backend.Application.Common;
using Backend.Application.Features.Identity;
using Backend.Domain.Enums;
using Microsoft.Extensions.Options;

namespace Backend.Tests.Features.Identity;

public sealed class RegistrationHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 19, 2, 0, 0, TimeSpan.Zero);

    private static (RegisterStudentHandler Handler, FakeUserRepository Users) CreateHandler(string? allowedDomain = null)
    {
        var users = new FakeUserRepository();
        var options = new OptionsWrapper<AuthOptions>(new AuthOptions { AllowedEmailDomain = allowedDomain });
        var handler = new RegisterStudentHandler(
            users, new FakeUnitOfWork(), new FakePasswordHasher(), new FixedTimeProvider(Now));
        // Validator is exercised through MediatR pipeline in production; construct it for direct checks.
        _ = new RegisterStudentValidator(options);
        return (handler, users);
    }

    private static RegisterStudentValidator Validator(string? allowedDomain = null) =>
        new(new OptionsWrapper<AuthOptions>(new AuthOptions { AllowedEmailDomain = allowedDomain }));

    [Fact]
    public async Task Register_Creates_Pending_Account_With_Trimmed_Control_Number()
    {
        var (handler, _) = CreateHandler();

        var dto = await handler.Handle(
            new RegisterStudentCommand("  s100 ", "Ana Paz", "ana@example.com", "secret123"),
            CancellationToken.None);

        Assert.Equal("s100", dto.ControlNumber);
        Assert.Equal(ApprovalStatus.Pending, dto.ApprovalStatus);
        Assert.True(dto.IsActive);
        Assert.Equal(Now, dto.CreatedAt);
    }

    [Fact]
    public async Task Register_Duplicate_Control_Number_Throws_Conflict()
    {
        var (handler, _) = CreateHandler();
        await handler.Handle(
            new RegisterStudentCommand("s101", "Ana Paz", "ana@example.com", "secret123"),
            CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(
            new RegisterStudentCommand("  s101 ", "Otro Nombre", "otro@example.com", "secret123"),
            CancellationToken.None));
    }

    [Fact]
    public async Task Register_Hashes_Password()
    {
        var (handler, users) = CreateHandler();
        await handler.Handle(
            new RegisterStudentCommand("s102", "Ana Paz", "ana@example.com", "secret123"),
            CancellationToken.None);

        var stored = await users.GetByControlNumberAsync("s102", CancellationToken.None);
        Assert.NotNull(stored);
        Assert.NotEqual("secret123", stored.PasswordHash);
        Assert.True(new FakePasswordHasher().Verify(stored.PasswordHash, "secret123"));
    }

    [Fact]
    public async Task Register_Dto_Never_Exposes_PasswordHash()
    {
        var (handler, _) = CreateHandler();
        var dto = await handler.Handle(
            new RegisterStudentCommand("s103", "Ana Paz", "ana@example.com", "secret123"),
            CancellationToken.None);

        Assert.DoesNotContain("secret123", dto.ToString());
        Assert.DoesNotContain("HASH", dto.ToString());
    }

    [Fact]
    public void Validator_Enforces_Allowed_Domain_Only_When_Configured()
    {
        var withDomain = Validator("tec.mx");
        Assert.True(withDomain.Validate(new RegisterStudentCommand("s1", "N", "a@tec.mx", "secret123")).IsValid);
        Assert.False(withDomain.Validate(new RegisterStudentCommand("s1", "N", "a@other.com", "secret123")).IsValid);

        var withoutDomain = Validator(null);
        Assert.True(withoutDomain.Validate(new RegisterStudentCommand("s1", "N", "a@other.com", "secret123")).IsValid);
    }

    [Fact]
    public void Validator_Rejects_Missing_Control_Number_And_Short_Password()
    {
        var validator = Validator(null);
        Assert.False(validator.Validate(new RegisterStudentCommand("   ", "N", "a@x.com", "secret123")).IsValid);
        Assert.False(validator.Validate(new RegisterStudentCommand("s1", "N", "a@x.com", "short")).IsValid);
    }
}
