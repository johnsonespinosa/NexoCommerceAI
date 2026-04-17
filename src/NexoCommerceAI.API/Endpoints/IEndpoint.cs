using Microsoft.AspNetCore.Routing;

namespace NexoCommerceAI.API.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
