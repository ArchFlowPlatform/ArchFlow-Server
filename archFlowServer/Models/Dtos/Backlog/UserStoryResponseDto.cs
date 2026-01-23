using archFlowServer.Models.Enums;

namespace archFlowServer.Models.Dtos.Backlog;

public class UserStoryResponseDto
{
    public int Id { get; set; }
    public int EpicId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Persona { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string AcceptanceCriteria { get; set; } = string.Empty;
    public UserStoryComplexity Complexity { get; set; }

    public int? Effort { get; set; }
    public string Dependencies { get; set; } = string.Empty;

    public int Priority { get; set; }
    public BusinessValue BusinessValue { get; set; }
    public UserStoryStatus Status { get; set; }
    public int Position { get; set; }
    public Guid? AssigneeId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
