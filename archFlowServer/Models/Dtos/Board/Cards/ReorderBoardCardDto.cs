using Swashbuckle.AspNetCore.Annotations;

namespace ArchFlowServer.Models.Dtos.Board.Cards
{
    public class ReorderBoardCardDto
    {
        [SwaggerSchema("Id do card (será sobrescrito pela rota).")]
        public int CardId { get; set; }

        [SwaggerSchema("Nova posição dentro da mesma coluna.")]
        public int ToPosition { get; set; }
    }
}
