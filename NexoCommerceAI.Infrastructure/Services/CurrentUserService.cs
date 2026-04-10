using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NexoCommerceAI.Application.Common.Interfaces;

namespace NexoCommerceAI.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId != null ? Guid.Parse(userId) : null;
        }
    }

    public string? Email => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
    
    public string? Username => httpContextAccessor.HttpContext?.User.FindFirstValue("username");
    
    public string? Role => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
    
    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
    
    public bool IsInRole(string role) => httpContextAccessor.HttpContext?.User.IsInRole(role) ?? false;
    
    public ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;
}