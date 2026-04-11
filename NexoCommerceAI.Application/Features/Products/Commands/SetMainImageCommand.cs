using MediatR;

namespace NexoCommerceAI.Application.Features.Products.Commands;

public class SetMainImageCommand : IRequest<bool>
{
    public Guid ProductId { get; set; }
    public Guid ImageId { get; set; }
}