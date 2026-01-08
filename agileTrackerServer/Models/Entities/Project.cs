using agileTrackerServer.Models.Enums;
using agileTrackerServer.Models.Exceptions;

namespace agileTrackerServer.Models.Entities;

public class Project
{
    private readonly List<ProjectMember> _members = new();

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ProjectStatus Status { get; private set; } = ProjectStatus.Active;
    public Guid OwnerId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // ✅ 1:1 ProductBacklog
    public ProductBacklog ProductBacklog { get; private set; } = null!;

    public IReadOnlyCollection<ProjectMember> Members => _members.AsReadOnly();

    private Project() { }

    public Project(string name, string? description, Guid ownerId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Nome do projeto é obrigatório.");

        if (ownerId == Guid.Empty)
            throw new DomainException("OwnerId inválido.");

        Id = Guid.NewGuid();
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Status = ProjectStatus.Active;
        OwnerId = ownerId;
        CreatedAt = DateTime.UtcNow;

        // 🔑 OWNER É CRIADO AQUI
        _members.Add(
            new ProjectMember(
                projectId: Id,
                userId: ownerId,
                role: MemberRole.Owner
            )
        );

        // ✅ PRODUCT BACKLOG É CRIADO JUNTO COM O PROJETO
        ProductBacklog = new ProductBacklog(projectId: Id);
    }

    // =========================
    // MEMBERSHIP RULES
    // =========================

    public void AddMember(Guid executorUserId, Guid newUserId, MemberRole role)
    {
        EnsurePermission(executorUserId, MemberRole.Owner);

        if (_members.Any(m => m.UserId == newUserId))
            throw new ConflictException("Usuário já é membro do projeto.");

        _members.Add(new ProjectMember(Id, newUserId, role));
    }

    public void RemoveMember(Guid executorUserId, Guid userId)
    {
        EnsurePermission(executorUserId, MemberRole.Owner);

        var member = _members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new DomainException("Usuário não é membro do projeto.");

        if (member.Role == MemberRole.Owner)
            throw new DomainException("Não é possível remover o Owner do projeto.");

        _members.Remove(member);
    }

    public bool HasPermission(Guid userId, params MemberRole[] roles)
        => _members.Any(m => m.UserId == userId && roles.Contains(m.Role));

    private void EnsurePermission(Guid userId, params MemberRole[] roles)
    {
        if (!HasPermission(userId, roles))
            throw new ForbiddenException("Permissão insuficiente para executar esta ação.");
    }

    public void UpdateDetails(Guid executorUserId, string name, string? description)
    {
        EnsurePermission(executorUserId, MemberRole.Owner);

        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Nome do projeto é obrigatório.");

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
    }

    public void Archive(Guid executorUserId)
    {
        EnsurePermission(executorUserId, MemberRole.Owner);

        Status = ProjectStatus.Archived;
    }
}
