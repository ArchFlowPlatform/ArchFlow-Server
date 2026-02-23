using archFlowServer.Models.Exceptions;

namespace archFlowServer.Models.Entities;

public enum StoryTaskStatus { Todo = 0, Doing = 1, Done = 2 }

public class StoryTask
{
    public int Id { get; private set; }

    // ✅ Task agora pertence ao SprintItem (story no sprint backlog)
    public int SprintItemId { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public Guid? AssigneeId { get; private set; }

    public int? EstimatedHours { get; private set; }
    public int? ActualHours { get; private set; }

    public int Priority { get; private set; } = 0;

    public int Position { get; private set; } = 0;
    public StoryTaskStatus Status { get; private set; } = StoryTaskStatus.Todo;

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // navigation
    public SprintItem SprintItem { get; private set; } = null!;

    private StoryTask() { } // EF

    internal StoryTask(
        int sprintItemId,
        string title,
        string? description,
        Guid? assigneeId,
        int? estimatedHours,
        int priority,
        int position,
        StoryTaskStatus status)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ValidationException("Título é obrigatório.");
        if (estimatedHours is < 0) throw new ValidationException("EstimatedHours não pode ser negativo.");
        if (priority < 0) throw new ValidationException("Priority inválida.");
        if (position < 0) throw new ValidationException("Position inválida.");

        SprintItemId = sprintItemId;

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

    public void SetPosition(int position)
    {
        if (position < 0) throw new ValidationException("Position inválida.");
        Position = position;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MoveToSprintItem(int sprintItemId)
    {
        SprintItemId = sprintItemId;
        UpdatedAt = DateTime.UtcNow;
    }
}