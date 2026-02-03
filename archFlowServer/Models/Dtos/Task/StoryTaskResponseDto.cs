namespace archFlowServer.Models.Dtos.Task;

public sealed record StoryTaskResponseDto(
    int Id,
    int UserStoryId,
    string Title,
    string Description,
    Guid? AssigneeId,
    int? EstimatedHours,
    int? ActualHours,
    int Priority,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
