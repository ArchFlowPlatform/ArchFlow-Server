using archFlowServer.Models.Enums;

namespace archFlowServer.Models.Dtos.Board.Cards;

public sealed record BoardCardResponseDto(
    int Id,
    int ColumnId,
    int? UserStoryId,
    int? StoryTaskId,
    string Title,
    string Description,
    Guid? AssigneeId,
    int Position,
    CardPriority Priority,
    DateTime? DueDate,
    int? EstimatedHours,
    int? ActualHours,
    string Color,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
