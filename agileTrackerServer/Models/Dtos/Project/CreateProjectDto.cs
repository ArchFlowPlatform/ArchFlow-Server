using Swashbuckle.AspNetCore.Annotations;

namespace agileTrackerServer.Models.Dtos.Project;

[SwaggerSchema(Description = "DTO para criação de um projeto.")]
public class CreateProjectDto
{
    [SwaggerSchema("Nome do projeto.")]
    public string Name { get; set; } = string.Empty;

    [SwaggerSchema("Descrição do projeto.")]
    public string Description { get; set; } = string.Empty;
}
