using archFlowServer.Models.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Models.Dtos.Project;

[SwaggerSchema(Description = "DTO retornado ao consultar um projeto.")]
public class ProjectResponseDto
{
    [SwaggerSchema("Identificador único do projeto.")]
    public Guid Id { get; set; }

    [SwaggerSchema("Nome do projeto.")]
    public string Name { get; set; } = string.Empty;

    [SwaggerSchema("Descrição detalhada do projeto.")]
    public string Description { get; set; } = string.Empty;

    [SwaggerSchema("ID do proprietário do projeto.")]
    public Guid OwnerId { get; set; }

    [SwaggerSchema("Nome do proprietário.")]
    public string OwnerName { get; set; } = string.Empty;

    [SwaggerSchema("Membros do projeto.")]
    public List<ProjectMemberResponseDto> Members { get; set; } = new();

    [SwaggerSchema("Status atual do projeto (ex.: Active, Archived).")]
    public ProjectStatus Status { get; set; }

    [SwaggerSchema("Data de criação do projeto.")]
    public DateTime CreatedAt { get; set; }
}
