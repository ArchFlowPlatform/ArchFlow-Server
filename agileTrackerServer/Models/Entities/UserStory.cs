using agileTrackerServer.Models.Enums;
using agileTrackerServer.Models.Exceptions;

namespace agileTrackerServer.Models.Entities;

public class UserStory
{
    public Guid Id { get; private set; }
    public Guid EpicId { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public string Persona { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public string AcceptanceCriteria { get; private set; } = string.Empty;
    public UserStoryComplexity Complexity { get; private set; } = UserStoryComplexity.Medium;

    public int? Effort { get; private set; } // story points
    public string Dependencies { get; private set; } = string.Empty;

    public int Priority { get; private set; } = 0;
    public BusinessValue BusinessValue { get; private set; } = BusinessValue.Medium;
    public UserStoryStatus Status { get; private set; } = UserStoryStatus.Draft;

    public Guid? AssigneeId { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation
    public Epic Epic { get; private set; } = null!;

    private UserStory() { } // EF

    internal UserStory(
        Guid epicId,
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
        Guid? assigneeId)
    {
        if (epicId == Guid.Empty)
            throw new DomainException("EpicId inválido.");

        if (string.IsNullOrWhiteSpace(title))
            throw new ValidationException("Título é obrigatório.");

        if (string.IsNullOrWhiteSpace(persona))
            throw new ValidationException("Persona é obrigatória.");

        if (string.IsNullOrWhiteSpace(description))
            throw new ValidationException("Descrição é obrigatória.");

        Id = Guid.NewGuid();
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

        AssigneeId = assigneeId;

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
        Guid? assigneeId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ValidationException("Título é obrigatório.");

        if (string.IsNullOrWhiteSpace(persona))
            throw new ValidationException("Persona é obrigatória.");

        if (string.IsNullOrWhiteSpace(description))
            throw new ValidationException("Descrição é obrigatória.");

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

        AssigneeId = assigneeId;

        UpdatedAt = DateTime.UtcNow;
    }
}
