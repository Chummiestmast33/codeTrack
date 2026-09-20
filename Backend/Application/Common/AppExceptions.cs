namespace Backend.Application.Common;

/// <summary>Typed application errors; an API middleware will map them to status codes later.</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string resource, object key)
        : base($"{resource} '{key}' was not found.")
    {
    }
}

public sealed class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    {
    }
}

public sealed class GoneException : Exception
{
    public GoneException(string message)
        : base(message)
    {
    }
}

/// <summary>Report header (Reports section) is missing. Message lists field
/// names only, never secret values, so it is safe to show.</summary>
public sealed class ReportsNotConfiguredException : InvalidOperationException
{
    public ReportsNotConfiguredException(string message)
        : base(message)
    {
    }
}

public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Invalid credentials.")
        : base(message)
    {
    }
}

public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string message)
        : base(message)
    {
    }
}
