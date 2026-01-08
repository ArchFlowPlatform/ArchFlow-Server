using agileTrackerServer.Models.Enums;
using agileTrackerServer.Models.Exceptions;

namespace agileTrackerServer.Models.Entities;

public class Epic
{
    public Guid Id { get; private set; }
    public Guid ProductBacklogId { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public BusinessValue BusinessValue { get; private set; } = BusinessValue.Medium;
    public EpicStatus Status { get; private set; } = EpicStatus.Draft;

    public int Priority { get; private set; } = 0;
    public string Color { get; private set; } = "#3498db";

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation
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
        int priority,
        string? color)
    {
        if (productBacklogId == Guid.Empty)
            throw new DomainException("ProductBacklogId inválido.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Nome do épico é obrigatório.");

        Id = Guid.NewGuid();
        ProductBacklogId = productBacklogId;

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;

        BusinessValue = businessValue;
        Status = status;
        Priority = priority;

        Color = string.IsNullOrWhiteSpace(color) ? "#3498db" : color.Trim();

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        string name,
        string? description,
        BusinessValue businessValue,
        EpicStatus status,
        int priority,
        string? color)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Nome do épico é obrigatório.");

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        BusinessValue = businessValue;
        Status = status;
        Priority = priority;

        if (!string.IsNullOrWhiteSpace(color))
            Color = color.Trim();

        UpdatedAt = DateTime.UtcNow;
    }
}
