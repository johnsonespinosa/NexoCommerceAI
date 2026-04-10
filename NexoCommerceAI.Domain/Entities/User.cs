using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Domain.Entities;

public class User : BaseEntity
{
    public string UserName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTime { get; private set; }
    public Guid RoleId { get; private set; }
    public Role Role { get; private set; } = default!;
    
    private User() { } // EF Core
    
    private User(string userName, string email, string passwordHash, Guid roleId)
    {
        UserName = userName;
        Email = email;
        PasswordHash = passwordHash;
        RoleId = roleId;
    }
    
    public static User Create(string userName, string email, string passwordHash, Guid roleId, 
        Func<string, Task<bool>>? isEmailUnique = null, 
        Func<string, Task<bool>>? isUserNameUnique = null)
    {
        ValidateUser(userName, email);
    
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required");
    
        // Validar unicidad de email
        if (isEmailUnique != null && !isEmailUnique(email).Result)
            throw new ArgumentException($"Email '{email}' is already registered");
    
        // Validar unicidad de username
        if (isUserNameUnique != null && !isUserNameUnique(userName).Result)
            throw new ArgumentException($"Username '{userName}' is already taken");
    
        return new User(userName, email, passwordHash, roleId);
    }
    
    public void UpdateProfile(string userName, string email)
    {
        ValidateUser(userName, email);
        
        UserName = userName;
        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public bool IsActiveUser()
    {
        return IsActive && !IsDeleted;
    }
    
    public bool IsRefreshTokenExpired()
    {
        return RefreshTokenExpiryTime <= DateTime.UtcNow;
    }
    
    public void UpdatePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required");
            
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateRole(Guid roleId)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException("Role id is required");
            
        RoleId = roleId;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SetRefreshToken(string refreshToken, DateTime expiryTime)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("Refresh token is required");
            
        if (expiryTime <= DateTime.UtcNow)
            throw new ArgumentException("Expiry time must be in the future");
            
        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = expiryTime;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public bool IsRefreshTokenValid(string refreshToken)
    {
        return !string.IsNullOrWhiteSpace(RefreshToken) &&
               RefreshToken == refreshToken &&
               RefreshTokenExpiryTime > DateTime.UtcNow;
    }
    
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SoftDelete()
    {
        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    private static void ValidateUser(string userName, string email)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("Username is required");
            
        if (userName.Length is < 3 or > 50)
            throw new ArgumentException("Username must be between 3 and 50 characters");
            
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required");
            
        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format");
    }
    
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}