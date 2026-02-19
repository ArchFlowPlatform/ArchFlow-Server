using archFlowServer.Models.Enums;
using archFlowServer.Models.Exceptions;

namespace archFlowServer.Models.Entities;

public class UserStory
{
    public int Id { get; private set; }
    public int EpicId { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public string Persona { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public string AcceptanceCriteria { get; private set; } = string.Empty;
    public UserStoryComplexity Complexity { get; private set; } = UserStoryComplexity.Medium;

    public int? Effort { get; private set; }
    public string Dependencies { get; private set; } = string.Empty;

    // ✅ prioridade do item (decisão de produto)
    public int Priority { get; private set; } = 0;

    public BusinessValue BusinessValue { get; private set; } = BusinessValue.Medium;
    public UserStoryStatus Status { get; private set; } = UserStoryStatus.Draft;

    // ✅ ordem no backlog (ranking / reorder do backlog)
    public int BacklogPosition { get; private set; }

    public Guid? AssigneeId { get; private set; }

    public bool IsArchived { get; private set; }
    public DateTime? ArchivedAt { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // navigations
    public Epic Epic { get; private set; } = null!;
    public ICollection<StoryTask> Tasks { get; private set; } = new List<StoryTask>();

    // ✅ se quiser rastrear em quantos boards ela está (opcional)
    public ICollection<BoardCard> BoardCards { get; private set; } = new List<BoardCard>();

    private UserStory() { } // EF

    internal UserStory(
        int epicId,
        string title,
        string persona,
        string description,
        string? acceptanceCriteria,
        UserStoryComplexity complexity,
        int? effort,
        string? dependencies,
        int priority,
        BusinessValue businessValue,
        UserStoryStatus status,
        int backlogPosition,
        Guid? assigneeId)
    {
        if (epicId <= 0) throw new DomainException("EpicId inválido.");
        if (string.IsNullOrWhiteSpace(title)) throw new ValidationException("Título é obrigatório.");
        if (string.IsNullOrWhiteSpace(persona)) throw new ValidationException("Persona é obrigatória.");
        if (string.IsNullOrWhiteSpace(description)) throw new ValidationException("Descrição é obrigatória.");
        if (backlogPosition < 0) throw new ValidationException("BacklogPosition inválida.");

        EpicId = epicId;

        Title = title.Trim();
        Persona = persona.Trim();
        Description = description.Trim();

        AcceptanceCriteria = acceptanceCriteria?.Trim() ?? string.Empty;
        Complexity = complexity;

        Effort = effort;
        Dependencies = dependencies?.Trim() ?? string.Empty;

        Priority = priority;
        BusinessValue = businessValue;
        Status = status;

        BacklogPosition = backlogPosition;
        AssigneeId = assigneeId;
        IsArchived = false;
        ArchivedAt = null;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        string title,
        string persona,
        string description,
        string? acceptanceCriteria,
        UserStoryComplexity complexity,
        int? effort,
        string? dependencies,
        int priority,
        BusinessValue businessValue,
        UserStoryStatus status,
        int backlogPosition,
        Guid? assigneeId)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ValidationException("Título é obrigatório.");
        if (string.IsNullOrWhiteSpace(persona)) throw new ValidationException("Persona é obrigatória.");
        if (string.IsNullOrWhiteSpace(description)) throw new ValidationException("Descrição é obrigatória.");
        if (backlogPosition < 0) throw new ValidationException("BacklogPosition inválida.");

        Title = title.Trim();
        Persona = persona.Trim();
        Description = description.Trim();

        AcceptanceCriteria = acceptanceCriteria?.Trim() ?? string.Empty;
        Complexity = complexity;

        Effort = effort;
        Dependencies = dependencies?.Trim() ?? string.Empty;

        Priority = priority;
        BusinessValue = businessValue;
        Status = status;

        BacklogPosition = backlogPosition;
        AssigneeId = assigneeId;

        UpdatedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        if (IsArchived) return;
        IsArchived = true;
        ArchivedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        if (!IsArchived) return;
        IsArchived = false;
        ArchivedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
