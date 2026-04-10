using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Application.Common.Interfaces;

public interface ISlugValidator
{
    Task<bool> IsSlugUniqueAsync<T>(string slug, Guid? excludeId = null) where T : BaseEntity;
}