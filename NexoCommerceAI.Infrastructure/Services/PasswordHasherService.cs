using NexoCommerceAI.Application.Common.Interfaces;

namespace NexoCommerceAI.Infrastructure.Services;

public class PasswordHashService : IPasswordHasherService
{
    public string Hash(string password)
    {
        throw new NotImplementedException();
    }

    public bool Verify(string password, string hashed)
    {
        throw new NotImplementedException();
    }
}