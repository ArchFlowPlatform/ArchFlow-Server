using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Models.Dtos.Sprint;

[SwaggerSchema(Description = "DTO para atualizar um item do sprint backlog (ordem e notas).")]
public class UpdateSprintItemDto
{
    [SwaggerSchema("Nova posição dentro do sprint backlog.")]
    public int Position { get; set; }

    [SwaggerSchema("Notas/plano de execução do item.")]
    public string? Notes { get; set; }
}
