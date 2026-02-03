namespace archFlowServer.Models.Dtos.Board.Cards;

public class MoveBoardCardDto
{
    public int ToColumnId { get; set; }
    public int? ToPosition { get; set; } // null -> final
}
