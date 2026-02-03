namespace ArchFlowServer.Models.Dtos.Board.Columns;

public class CreateBoardColumnDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int? Position { get; set; } // null -> final
    public int? WipLimit { get; set; }

    public string? Color { get; set; }
    public bool IsDoneColumn { get; set; } = false;
}
