using agileTrackerServer.Models.Enums;

namespace agileTrackerServer.Models.Dtos.Backlog;

public class UserStoryResponseDto
{
    public Guid Id { get; set; }
    public Guid EpicId { get; set; }

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

    public Guid? AssigneeId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}