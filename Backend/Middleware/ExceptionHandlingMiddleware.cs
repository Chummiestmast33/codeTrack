using Backend.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Middleware;

/// <summary>Global error mapping to RFC 7807 ProblemDetails. Never leaks secrets.</summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await WriteProblemAsync(context, ex);
        }
    }

    private async Task WriteProblemAsync(HttpContext context, Exception ex)
    {
        var (status, title, errors) = Map(ex);

        if (status >= 500)
        {
            _logger.LogError(ex, "Unhandled error processing {Method} {Path}", context.Request.Method, context.Request.Path);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status >= 500 ? "An unexpected error occurred." : ex.Message,
            Instance = context.Request.Path
        };

        if (errors is not null)
        {
            problem.Extensions["errors"] = errors;
        }

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(
            problem,
            new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web),
            "application/problem+json",
            context.RequestAborted);
    }

    internal static (int Status, string Title, IReadOnlyDictionary<string, string[]>? Errors) Map(Exception ex) =>
        ex switch
        {
            FluentValidation.ValidationException vex => (
                StatusCodes.Status400BadRequest,
                "Validation failed.",
                vex.Errors
                    .GroupBy(f => f.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray())
                as IReadOnlyDictionary<string, string[]>),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized.", null),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden.", null),
            NotFoundException => (StatusCodes.Status404NotFound, "Not found.", null),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict.", null),
            GoneException => (StatusCodes.Status410Gone, "Gone.", null),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid input.", null),
            _ => (StatusCodes.Status500InternalServerError, "Server error.", null)
        };
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<ExceptionHandlingMiddleware>();
}
