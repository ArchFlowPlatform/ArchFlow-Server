using archFlowServer.Models.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Models.Dtos.Board.Cards;

public class CreateBoardCardDto
{
    [SwaggerSchema("Id da user story para criar o card no Kanban.")]
    public int UserStoryId { get; set; }

    [SwaggerSchema("Posição desejada (opcional). Se null, entra no final.")]
    public int? Position { get; set; }
}
