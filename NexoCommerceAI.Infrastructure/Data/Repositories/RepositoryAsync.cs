using Ardalis.Specification.EntityFrameworkCore;
using NexoCommerceAI.Application.Common.Interfaces;

namespace NexoCommerceAI.Infrastructure.Data.Repositories;

public class RepositoryAsync<T>(ApplicationDbContext dbContext) : RepositoryBase<T>(dbContext), IRepositoryAsync<T>
    where T : class;