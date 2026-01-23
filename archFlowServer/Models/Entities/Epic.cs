using archFlowServer.Models.Enums;
using archFlowServer.Models.Exceptions;

namespace archFlowServer.Models.Entities;

public class Epic
{
    public int Id { get; private set; }
    public Guid ProductBacklogId { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public BusinessValue BusinessValue { get; private set; } = BusinessValue.Medium;
    public EpicStatus Status { get; private set; } = EpicStatus.Draft;

    public int Position { get; private set; }
    public int Priority { get; private set; } = 0;
    public string Color { get; private set; } = "#3498db";
    public bool IsArchived { get; private set; }
    public DateTime? ArchivedAt { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public ProductBacklog ProductBacklog { get; private set; } = null!;

    private readonly List<UserStory> _userStories = new();
    public IReadOnlyCollection<UserStory> UserStories => _userStories.AsReadOnly();

    private Epic() { } // EF

    internal Epic(
        Guid productBacklogId,
        string name,
        string? description,
        BusinessValue businessValue,
        EpicStatus status,
        int position,
        int priority,
        string? color,
        bool isArchived,
        DateTime? archivedAt)
    {
        if (productBacklogId == Guid.Empty)
            throw new DomainException("ProductBacklogId inválido.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Nome do Ã©pico Ã© obrigatÃ³rio.");

        if (position < 0)
            throw new ValidationException("Position invÃ¡lida.");

        ProductBacklogId = productBacklogId;
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;

        BusinessValue = businessValue;
        Status = status;

        Position = position;
        Priority = priority;
        Color = string.IsNullOrWhiteSpace(color) ? "#3498db" : color.Trim();
        
        IsArchived = isArchived;
        ArchivedAt = archivedAt;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        string name,
        string? description,
        BusinessValue businessValue,
        EpicStatus status,
        int position,
        int priority,
        string? color)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Nome do Ã©pico Ã© obrigatÃ³rio.");

        if (position < 0)
            throw new ValidationException("Position invÃ¡lida.");

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;

        BusinessValue = businessValue;
        Status = status;

        Position = position;
        Priority = priority;

        if (!string.IsNullOrWhiteSpace(color))
            Color = color.Trim();

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

