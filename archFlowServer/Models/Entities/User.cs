using archFlowServer.Models.Enums;
using archFlowServer.Models.Exceptions;

namespace archFlowServer.Models.Entities;

public class User
{
    // ============================
    // PROPERTIES (EF Core)
    // ============================
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public UserType Type { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;
    public string AvatarUrl { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // ============================
    // CONSTRUCTOR (EF Core)
    // ============================
    protected User() { }

    // ============================
    // CONSTRUCTOR (DOMAIN)
    // ============================
    public User(
        string name,
        string email,
        UserType type,
        string passwordHash,
        string? avatarUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Nome do usuário Ã© obrigatÃ³rio.");

        if (string.IsNullOrWhiteSpace(email))
            throw new ValidationException("Email do usuário Ã© obrigatÃ³rio.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ValidationException("Senha do usuário Ã© obrigatÃ³ria.");

        Id = Guid.NewGuid();
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        Type = type;
        PasswordHash = passwordHash;
        AvatarUrl = avatarUrl?.Trim() ?? string.Empty;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // ============================
    // DOMAIN BEHAVIOR
    // ============================
    public void UpdateProfile(string name, string? avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Nome do usuário Ã© obrigatÃ³rio.");

        Name = name.Trim();
        AvatarUrl = avatarUrl?.Trim() ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ValidationException("Senha invÃ¡lida.");

        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeType(UserType newType)
    {
        Type = newType;
        UpdatedAt = DateTime.UtcNow;
    }
}

