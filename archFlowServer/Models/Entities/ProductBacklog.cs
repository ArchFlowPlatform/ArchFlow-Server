using archFlowServer.Models.Exceptions;

namespace archFlowServer.Models.Entities;

public class ProductBacklog
{
    
    private readonly List<Epic> _epics = new();
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public string Overview { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation (EF)
    public IReadOnlyCollection<Epic> Epics => _epics.AsReadOnly();
    public Project Project { get; private set; } = null!;

    private ProductBacklog() { } // EF

    internal ProductBacklog(Guid projectId, string? overview = null)
    {
        if (projectId == Guid.Empty)
            throw new DomainException("ProjectId inválido.");

        Id = Guid.NewGuid();
        ProjectId = projectId;

        Overview = overview?.Trim() ?? string.Empty;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateOverview(string? overview)
    {
        Overview = overview?.Trim() ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }
}
