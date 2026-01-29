using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Models.Dtos.Sprint;

[SwaggerSchema(Description = "DTO para adicionar um item ao sprint backlog.")]
public class CreateSprintItemDto
{
    [SwaggerSchema("Id da UserStory que será adicionada ao sprint.")]
    public int UserStoryId { get; set; }

    [SwaggerSchema("Posição opcional dentro do sprint backlog. Se null, adiciona no final.")]
    public int? Position { get; set; }

    [SwaggerSchema("Notas/plano de execução para este item dentro da sprint.")]
    public string? Notes { get; set; }
}
