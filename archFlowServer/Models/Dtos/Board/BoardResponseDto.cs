using archFlowServer.Models.Enums;

namespace archFlowServer.Models.Dtos.Board;

public sealed record BoardResponseDto(
    Guid Id,
    Guid ProjectId,
    Guid SprintId,
    string Name,
    string Description,
    BoardType BoardType,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
