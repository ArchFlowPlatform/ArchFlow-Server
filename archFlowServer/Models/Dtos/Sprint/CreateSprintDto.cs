using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Models.Dtos.Sprint;

[SwaggerSchema(Description = "DTO para criar um sprint.")]
public class CreateSprintDto
{
    [SwaggerSchema("ID do projeto ao qual o sprint pertence.")]
    public Guid ProjectId { get; set; }

    [SwaggerSchema("Nome do sprint.")]
    public string Name { get; set; } = string.Empty;

    [SwaggerSchema("Meta principal do sprint.")]
    public string Goal { get; set; } = string.Empty;

    [SwaggerSchema("Data de inÃ­cio.")]
    public DateTime StartDate { get; set; }

    [SwaggerSchema("Data de tÃ©rmino.")]
    public DateTime EndDate { get; set; }

    [SwaggerSchema("Capacidade total em horas do sprint.")]
    public int CapacityHours { get; set; }
}

