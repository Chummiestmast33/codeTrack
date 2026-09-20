using System.Security.Claims;
using Backend.Application.Common;
using Backend.Application.Features.Identity;
using Backend.Middleware;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace Backend.Tests.Api;

public sealed class ExceptionHandlingTests
{
    private static async Task<(int Status, string ContentType, string Body)> RunAsync(Exception error)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/auth/login";
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => Task.FromException(error), NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return (context.Response.StatusCode, context.Response.ContentType ?? "", await reader.ReadToEndAsync());
    }

    [Theory]
    [InlineData(typeof(FluentValidation.ValidationException), 400)]
    [InlineData(typeof(UnauthorizedException), 401)]
    [InlineData(typeof(ForbiddenException), 403)]
    [InlineData(typeof(NotFoundException), 404)]
    [InlineData(typeof(ConflictException), 409)]
    [InlineData(typeof(GoneException), 410)]
    [InlineData(typeof(InvalidOperationException), 500)]
    public async Task Maps_Exceptions_To_Status_Codes(Type errorType, int expectedStatus)
    {
        Exception error = errorType switch
        {
            _ when errorType == typeof(FluentValidation.ValidationException) =>
                new FluentValidation.ValidationException(new[] { new ValidationFailure("Email", "bad") }),
            _ when errorType == typeof(NotFoundException) => new NotFoundException("User", "x"),
            _ when errorType == typeof(ConflictException) => new ConflictException("dup"),
            _ when errorType == typeof(GoneException) => new GoneException("expired"),
            _ when errorType == typeof(UnauthorizedException) => new UnauthorizedException(),
            _ when errorType == typeof(ForbiddenException) => new ForbiddenException("no"),
            _ => new InvalidOperationException("boom")
        };

        var (status, contentType, body) = await RunAsync(error);

        Assert.Equal(expectedStatus, status);
        Assert.Equal("application/problem+json", contentType);
        Assert.Contains("\"status\":" + expectedStatus, body);
    }

    [Fact]
    public async Task Server_Error_Hides_Details_And_Leaks_No_Secrets()
    {
        var (status, _, body) = await RunAsync(new InvalidOperationException("secret123 connstring"));

        Assert.Equal(500, status);
        Assert.DoesNotContain("secret123", body);
        Assert.Contains("An unexpected error occurred.", body);
    }

    [Fact]
    public async Task Validation_Error_Includes_Errors_Extension()
    {
        var (status, _, body) = await RunAsync(
            new FluentValidation.ValidationException(new[] { new ValidationFailure("Password", "too short") }));

        Assert.Equal(400, status);
        Assert.Contains("errors", body);
        Assert.Contains("Password", body);
    }

    [Fact]
    public async Task Me_Without_Parsable_Sub_Returns_Unauthorized()
    {
        var controller = new Controllers.UsersController(new CapturingSender(_ => Task.FromResult<object?>(new UserDto(
            Guid.NewGuid(), "s1", "N", "e@x.com",
            Domain.Enums.UserRole.Student, Domain.Enums.ApprovalStatus.Approved, true, DateTimeOffset.UtcNow))));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        var result = await controller.Me(CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task Me_With_Sub_Queries_By_Id()
    {
        var userId = Guid.NewGuid();
        var sender = new CapturingSender(_ => Task.FromResult<object?>(new UserDto(
            userId, "s1", "N", "e@x.com",
            Domain.Enums.UserRole.Student, Domain.Enums.ApprovalStatus.Approved, true, DateTimeOffset.UtcNow)));
        var controller = new Controllers.UsersController(sender);
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) });
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        var result = await controller.Me(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var query = Assert.IsType<GetUserByIdQuery>(sender.LastRequest);
        Assert.Equal(userId, query.UserId);
        Assert.Equal(userId, ((UserDto)ok.Value!).Id);
    }
}
