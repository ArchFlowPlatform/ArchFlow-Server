using archFlowServer.Models.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Models.Dtos.Board.Cards;

public record BoardCardResponseDto(
    [property: SwaggerSchema("Id do card.")] int Id,
    [property: SwaggerSchema("Id da coluna do board.")] int ColumnId,
    [property: SwaggerSchema("Id da user story representada pelo card.")] int UserStoryId,
    [property: SwaggerSchema("Posição do card dentro da coluna (ordenação drag).")] int Position,
    [property: SwaggerSchema("Data de criação (UTC).")] DateTime CreatedAt,
    [property: SwaggerSchema("Data de atualização (UTC).")] DateTime UpdatedAt
);
