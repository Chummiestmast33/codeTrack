using System.Security.Claims;
using Backend.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Backend.Infrastructure.Security;

public sealed class HttpUserContext : IUserContext
{
    private readonly IHttpContextAccessor _accessor;

    public HttpUserContext(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public Guid? CurrentUserId
    {
        get
        {
            var sub = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? _accessor.HttpContext?.User.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }
}
