using archFlowServer.Models.Exceptions;

namespace archFlowServer.Models.Entities;

public enum StoryTaskStatus { Todo = 0, Doing = 1, Done = 2 }

public class StoryTask
{
    public int Id { get; private set; }
    public int UserStoryId { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public Guid? AssigneeId { get; private set; }

    public int? EstimatedHours { get; private set; }
    public int? ActualHours { get; private set; }

    public int Priority { get; private set; } = 0;

    // ✅ recomendado (ordem do checklist)
    public int Position { get; private set; } = 0;

    // ✅ recomendado
    public StoryTaskStatus Status { get; private set; } = StoryTaskStatus.Todo;

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public UserStory UserStory { get; private set; } = null!;

    private StoryTask() { } // EF

    internal StoryTask(
        int userStoryId,
        string title,
        string? description,
        Guid? assigneeId,
        int? estimatedHours,
        int priority,
        int position,
        StoryTaskStatus status)
    {
        if (userStoryId <= 0) throw new DomainException("UserStoryId inválido.");
        if (string.IsNullOrWhiteSpace(title)) throw new ValidationException("Título é obrigatório.");
        if (estimatedHours is < 0) throw new ValidationException("EstimatedHours não pode ser negativo.");
        if (priority < 0) throw new ValidationException("Priority inválida.");
        if (position < 0) throw new ValidationException("Position inválida.");

        UserStoryId = userStoryId;
        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;

        AssigneeId = assigneeId;
        EstimatedHours = estimatedHours;
        Priority = priority;

        Position = position;
        Status = status;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        string title,
        string? description,
        Guid? assigneeId,
        int? estimatedHours,
        int? actualHours,
        int priority,
        StoryTaskStatus status)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ValidationException("Título é obrigatório.");
        if (estimatedHours is < 0) throw new ValidationException("EstimatedHours não pode ser negativo.");
        if (actualHours is < 0) throw new ValidationException("ActualHours não pode ser negativo.");
        if (priority < 0) throw new ValidationException("Priority inválida.");

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;

        AssigneeId = assigneeId;
        EstimatedHours = estimatedHours;
        ActualHours = actualHours;
        Priority = priority;

        Status = status;

        UpdatedAt = DateTime.UtcNow;
    }
}
