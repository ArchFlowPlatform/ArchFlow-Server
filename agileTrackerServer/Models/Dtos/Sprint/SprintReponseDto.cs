using Swashbuckle.AspNetCore.Annotations;

namespace agileTrackerServer.Models.Dtos.Sprint;

[SwaggerSchema(Description = "DTO retornado ao consultar um sprint.")]
public class SprintResponseDto
{
    [SwaggerSchema("Identificador único do sprint.")]
    public Guid Id { get; set; }

    [SwaggerSchema("ID do projeto associado.")]
    public Guid ProjectId { get; set; }

    [SwaggerSchema("Nome do sprint.")]
    public string Name { get; set; } = string.Empty;

    [SwaggerSchema("Meta do sprint.")]
    public string Goal { get; set; } = string.Empty;

    [SwaggerSchema("Status atual do sprint.")]
    public string Status { get; set; } = string.Empty;

    [SwaggerSchema("Capacidade total em horas.")]
    public int CapacityHours { get; set; }

    [SwaggerSchema("Data de início.")]
    public DateTime StartDate { get; set; }

    [SwaggerSchema("Data de término.")]
    public DateTime EndDate { get; set; }

    [SwaggerSchema("Data de criação do sprint.")]
    public DateTime CreatedAt { get; set; }

    [SwaggerSchema("Data da última atualização.")]
    public DateTime UpdatedAt { get; set; }
}
