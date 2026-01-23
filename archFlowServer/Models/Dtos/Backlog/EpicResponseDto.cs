using archFlowServer.Models.Enums;

namespace archFlowServer.Models.Dtos.Backlog;

public class EpicResponseDto
{
    public int Id { get; set; }
    public Guid ProductBacklogId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public BusinessValue BusinessValue { get; set; }
    public EpicStatus Status { get; set; }
    public int Position { get; set; }
    public int Priority { get; set; }
    public string Color { get; set; } = "#3498db";

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<UserStoryResponseDto> UserStories { get; set; } = new();
}
