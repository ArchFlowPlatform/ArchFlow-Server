namespace archFlowServer.Models.Dtos.Labels;

public sealed record LabelResponseDto(
    int Id,
    Guid ProjectId,
    string Name,
    string Color,
    DateTime CreatedAt,
    DateTime UpdatedAt
);