using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Models.Dtos.Task;

public class CreateStoryTaskDto
{
    [SwaggerSchema("Título da task.")]
    public string Title { get; set; } = string.Empty;

    [SwaggerSchema("Descrição da task.")]
    public string? Description { get; set; }

    [SwaggerSchema("Responsável (Guid).")]
    public Guid? AssigneeId { get; set; }

    [SwaggerSchema("Horas estimadas.")]
    public int? EstimatedHours { get; set; }

    [SwaggerSchema("Prioridade numérica. 0 = padrão.")]
    public int Priority { get; set; } = 0;
}

