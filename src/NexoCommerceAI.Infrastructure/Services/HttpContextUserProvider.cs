using Microsoft.AspNetCore.Http;

namespace NexoCommerceAI.Infrastructure.Services;

internal sealed class HttpContextUserProvider(IHttpContextAccessor httpContextAccessor) : Persistence.IUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public string? GetUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return null;
        var sub = user.FindFirst("sub")?.Value ?? user.FindFirst("nameidentifier")?.Value;
        return sub;
    }
}
