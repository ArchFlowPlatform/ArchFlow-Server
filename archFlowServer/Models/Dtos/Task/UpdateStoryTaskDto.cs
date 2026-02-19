using archFlowServer.Models.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Models.Dtos.Task;

public class UpdateStoryTaskDto
{
    [SwaggerSchema("Título da task.")]
    public string Title { get; set; } = string.Empty;

    [SwaggerSchema("Descrição da task.")]
    public string? Description { get; set; }

    [SwaggerSchema("Responsável (Guid).")]
    public Guid? AssigneeId { get; set; }

    [SwaggerSchema("Horas estimadas.")]
    public int? EstimatedHours { get; set; }

    [SwaggerSchema("Horas reais realizadas.")]
    public int? ActualHours { get; set; }

    [SwaggerSchema("Prioridade numérica.")]
    public int Priority { get; set; } = 0;

    [SwaggerSchema("Status da Task.")]
    public StoryTaskStatus Status { get; set; } = 0;
}
