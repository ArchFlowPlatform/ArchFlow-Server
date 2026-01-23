using archFlowServer.Models.Enums;

namespace archFlowServer.Models.Dtos.Backlog;

public class CreateUserStoryDto
{
    public string Title { get; set; } = string.Empty;
    public string Persona { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? AcceptanceCriteria { get; set; }

    public UserStoryComplexity Complexity { get; set; } = UserStoryComplexity.Medium;
    public int? Effort { get; set; }

    public string? Dependencies { get; set; }
    public int Priority { get; set; } = 0;

    public BusinessValue BusinessValue { get; set; } = BusinessValue.Medium;
    public UserStoryStatus Status { get; set; } = UserStoryStatus.Draft;
    public Guid? AssigneeId { get; set; }
    public bool IsArchived { get; set; } = false;
}
