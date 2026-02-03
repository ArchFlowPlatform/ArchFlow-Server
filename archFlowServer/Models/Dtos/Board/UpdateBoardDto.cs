using archFlowServer.Models.Enums;

namespace archFlowServer.Models.Dtos.Board;

public class UpdateBoardDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public BoardType BoardType { get; set; } = BoardType.Kanban;
}
