using archFlowServer.Models.Enums;
using archFlowServer.Models.Exceptions;

namespace archFlowServer.Models.Entities;

public class BoardCard
{
    public int Id { get; private set; }

    public int ColumnId { get; private set; }

    public int? UserStoryId { get; private set; }
    public int? StoryTaskId { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public Guid? AssigneeId { get; private set; }

    public int Position { get; private set; }
    public CardPriority Priority { get; private set; } = CardPriority.Medium;

    public DateTime? DueDate { get; private set; }

    public int? EstimatedHours { get; private set; }
    public int? ActualHours { get; private set; }

    public string Color { get; private set; } = "#ffffff";

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // navigations
    public BoardColumn Column { get; private set; } = null!;
    public UserStory? UserStory { get; private set; }
    public StoryTask? StoryTask { get; private set; }

    private BoardCard() { } // EF

    internal BoardCard(
        int columnId,
        int position,
        string title,
        string? description,
        int? userStoryId,
        int? storyTaskId,
        Guid? assigneeId,
        CardPriority priority,
        DateTime? dueDate,
        int? estimatedHours,
        string? color)
    {
        if (columnId <= 0)
            throw new DomainException("ColumnId inválido.");

        if (position < 0)
            throw new ValidationException("Position inválida.");

        if (string.IsNullOrWhiteSpace(title))
            throw new ValidationException("Título é obrigatório.");

        ValidateOrigin(userStoryId, storyTaskId);

        if (estimatedHours is < 0)
            throw new ValidationException("EstimatedHours não pode ser negativo.");

        ColumnId = columnId;
        Position = position;

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;

        UserStoryId = userStoryId;
        StoryTaskId = storyTaskId;

        AssigneeId = assigneeId;
        Priority = priority;
        DueDate = dueDate;

        EstimatedHours = estimatedHours;
        Color = NormalizeHex(color);
    }

    public void Update(
        string title,
        string? description,
        Guid? assigneeId,
        CardPriority priority,
        DateTime? dueDate,
        int? estimatedHours,
        int? actualHours,
        string? color)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ValidationException("Título é obrigatório.");

        if (estimatedHours is < 0)
            throw new ValidationException("EstimatedHours não pode ser negativo.");

        if (actualHours is < 0)
            throw new ValidationException("ActualHours não pode ser negativo.");

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;

        AssigneeId = assigneeId;
        Priority = priority;
        DueDate = dueDate;

        EstimatedHours = estimatedHours;
        ActualHours = actualHours;

        Color = NormalizeHex(color);
    }

    public void SetPosition(int position)
    {
        if (position < 0)
            throw new ValidationException("Position inválida.");

        Position = position;
    }

    private static void ValidateOrigin(int? userStoryId, int? storyTaskId)
    {
        var hasStory = userStoryId.HasValue;
        var hasTask = storyTaskId.HasValue;

        if (hasStory == hasTask) // ambos true ou ambos false
            throw new ValidationException("Card deve referenciar exatamente uma origem: UserStoryId ou StoryTaskId.");

        if (userStoryId is <= 0)
            throw new ValidationException("UserStoryId inválido.");

        if (storyTaskId is <= 0)
            throw new ValidationException("StoryTaskId inválido.");
    }

    private static string NormalizeHex(string? color)
    {
        var c = (color ?? "#ffffff").Trim();
        if (c.Length != 7 || c[0] != '#')
            throw new ValidationException("Color inválida. Use formato #RRGGBB.");

        return c.ToLowerInvariant();
    }
}
