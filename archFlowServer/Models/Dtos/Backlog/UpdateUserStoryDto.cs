using archFlowServer.Models.Enums;

namespace archFlowServer.Models.Dtos.Backlog;

public class UpdateUserStoryDto
{
    public string? Title { get; set; }
    public string? Persona { get; set; }
    public string? Description { get; set; }

    public string? AcceptanceCriteria { get; set; }
    public UserStoryComplexity? Complexity { get; set; }

    public int? Effort { get; set; }
    public string? Dependencies { get; set; }

    public int? Priority { get; set; }
    public BusinessValue? BusinessValue { get; set; }
    public UserStoryStatus? Status { get; set; }
    public int? Position { get; set; }
    public Guid? AssigneeId { get; set; }
}
