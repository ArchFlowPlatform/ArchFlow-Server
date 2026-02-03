using archFlowServer.Models.Enums;

namespace archFlowServer.Models.Dtos.Board.Cards;

public class CreateBoardCardDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int? UserStoryId { get; set; }
    public int? StoryTaskId { get; set; }

    public Guid? AssigneeId { get; set; }

    public int? Position { get; set; } // null -> final
    public CardPriority Priority { get; set; } = CardPriority.Medium;

    public DateTime? DueDate { get; set; }
    public int? EstimatedHours { get; set; }

    public string? Color { get; set; }
}
