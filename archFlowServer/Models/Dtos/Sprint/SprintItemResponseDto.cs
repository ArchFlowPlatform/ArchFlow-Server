using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Models.Dtos.Sprint;

[SwaggerSchema(Description = "DTO retornado ao consultar itens do sprint backlog.")]
public sealed record SprintItemResponseDto(
    int Id,
    Guid SprintId,
    int UserStoryId,
    int Position,
    string Notes,
    DateTime AddedAt
);
