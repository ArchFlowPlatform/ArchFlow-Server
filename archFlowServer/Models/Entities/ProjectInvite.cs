using archFlowServer.Models.Enums;
using archFlowServer.Models.Exceptions;

namespace archFlowServer.Models.Entities;

public class ProjectInvite
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public MemberRole Role { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool Accepted { get; private set; }

    private ProjectInvite() { }

    public ProjectInvite(
        Guid projectId,
        string email,
        MemberRole role,
        TimeSpan expiration)
    {
        if (projectId == Guid.Empty)
            throw new ValidationException("Projeto inválido.");

        if (string.IsNullOrWhiteSpace(email))
            throw new ValidationException("Email inválido.");

        Id = Guid.NewGuid();
        ProjectId = projectId;
        Email = email.Trim().ToLower();
        Role = role;
        Token = Guid.NewGuid().ToString("N");
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = CreatedAt.Add(expiration);
        Accepted = false;
    }

    public void Accept()
    {
        if (Accepted)
            throw new ConflictException("Convite jÃ¡ foi aceito.");

        if (DateTime.UtcNow > ExpiresAt)
            throw new DomainException("Convite expirado.");

        Accepted = true;
    }
}

