using archFlowServer.Models.Contracts;
using archFlowServer.Models.Enums;
using archFlowServer.Models.Exceptions;

namespace archFlowServer.Models.Entities;

public class Board : IAuditableEntity
{
    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }
    public Guid SprintId { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public BoardType BoardType { get; private set; } = BoardType.Kanban;

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // navigations
    public Project Project { get; private set; } = null!;
    public Sprint Sprint { get; private set; } = null!;

    private Board() { } // EF

    internal Board(
        Guid projectId,
        Guid sprintId,
        string name,
        string? description,
        BoardType boardType)
    {
        if (projectId == Guid.Empty)
            throw new DomainException("ProjectId inválido.");

        if (sprintId == Guid.Empty)
            throw new DomainException("SprintId inválido.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Nome do board é obrigatório.");

        Id = Guid.NewGuid();
        ProjectId = projectId;
        SprintId = sprintId;

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        BoardType = boardType;
    }

    public void Update(string name, string? description, BoardType boardType)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Nome do board é obrigatório.");

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        BoardType = boardType;

        // UpdatedAt é responsabilidade do interceptor
    }

    // ===== IAuditableEntity =====
    public void SetCreatedAt(DateTime utcNow) => CreatedAt = utcNow;
    public void SetUpdatedAt(DateTime utcNow) => UpdatedAt = utcNow;
}
