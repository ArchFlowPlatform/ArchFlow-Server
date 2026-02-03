namespace ArchFlowServer.Models.Dtos.Board.Columns;

public class UpdateBoardColumnDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int? WipLimit { get; set; }
    public string? Color { get; set; }

    public bool IsDoneColumn { get; set; } = false;
}
