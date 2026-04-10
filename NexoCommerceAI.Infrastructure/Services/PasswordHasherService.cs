using Microsoft.AspNetCore.Identity;
using NexoCommerceAI.Application.Common.Interfaces;

namespace NexoCommerceAI.Infrastructure.Services;

public class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<object> _hasher = new();
    public string Hash(string password) => _hasher.HashPassword(null!, password);
    public bool Verify(string password, string hashed) =>
        _hasher.VerifyHashedPassword(null!, hashed, password) == PasswordVerificationResult.Success;
}