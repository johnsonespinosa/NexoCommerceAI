using Ardalis.Specification;

namespace NexoCommerceAI.Application.Common.Interfaces;

public interface IRepositoryAsync<TEntity> : IRepositoryBase<TEntity> where TEntity : class;