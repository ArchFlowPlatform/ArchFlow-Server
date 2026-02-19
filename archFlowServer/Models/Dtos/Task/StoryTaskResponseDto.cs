using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Models.Dtos.Task;

public record StoryTaskResponseDto(
    [property: SwaggerSchema("Id da task.")] int Id,
    [property: SwaggerSchema("Id da user story pai.")] int UserStoryId,
    [property: SwaggerSchema("Posição (0-based) dentro da user story.")] int Position,
    [property: SwaggerSchema("Título.")] string Title,
    [property: SwaggerSchema("Descrição.")] string Description,
    [property: SwaggerSchema("Responsável (Guid).")] Guid? AssigneeId,
    [property: SwaggerSchema("Horas estimadas.")] int? EstimatedHours,
    [property: SwaggerSchema("Horas reais.")] int? ActualHours,
    [property: SwaggerSchema("Prioridade numérica.")] int Priority,
    [property: SwaggerSchema("Criado em UTC.")] DateTime CreatedAt,
    [property: SwaggerSchema("Atualizado em UTC.")] DateTime UpdatedAt
);
