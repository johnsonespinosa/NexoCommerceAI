namespace NexoCommerceAI.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? GetCurrentUserId();
    string? GetCurrentUserName();
    string? GetCurrentUserEmail();
    string? GetCurrentIpAddress();
    string? GetCurrentUserAgent();
    Guid? GetCorrelationId();
}
