using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Models.Dtos.Board.Cards;

public class MoveBoardCardDto
{
    [SwaggerSchema("Coluna de destino.")]
    public int ToColumnId { get; set; }

    [SwaggerSchema("Posição de destino (opcional). Se null, entra no final da coluna.")]
    public int? ToPosition { get; set; }
}
