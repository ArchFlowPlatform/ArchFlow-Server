namespace ArchFlowServer.Models.Dtos.Board.Columns;

public sealed record BoardColumnResponseDto(
    int Id,
    Guid BoardId,
    string Name,
    string Description,
    int Position,
    int? WipLimit,
    string Color,
    bool IsDoneColumn,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
