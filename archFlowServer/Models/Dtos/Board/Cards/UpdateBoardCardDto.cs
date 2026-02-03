using archFlowServer.Models.Enums;

namespace archFlowServer.Models.Dtos.Board.Cards;

public class UpdateBoardCardDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Guid? AssigneeId { get; set; }

    public CardPriority Priority { get; set; } = CardPriority.Medium;

    public DateTime? DueDate { get; set; }
    public int? EstimatedHours { get; set; }
    public int? ActualHours { get; set; }

    public string? Color { get; set; }
}
