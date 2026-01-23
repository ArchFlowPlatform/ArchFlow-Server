using archFlowServer.Models.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Models.Dtos.Project;

[SwaggerSchema(Description = "DTO retornado ao consultar um projeto.")]
public class ProjectResponseDto
{
    [SwaggerSchema("Identificador Ãºnico do projeto.")]
    public Guid Id { get; set; }

    [SwaggerSchema("Nome do projeto.")]
    public string Name { get; set; } = string.Empty;

    [SwaggerSchema("DescriÃ§Ã£o detalhada do projeto.")]
    public string Description { get; set; } = string.Empty;

    [SwaggerSchema("ID do proprietÃ¡rio do projeto.")]
    public Guid OwnerId { get; set; }

    [SwaggerSchema("Nome do proprietÃ¡rio.")]
    public string OwnerName { get; set; } = string.Empty;

    [SwaggerSchema("Status atual do projeto (ex.: Active, Archived).")]
    public ProjectStatus Status { get; set; }

    [SwaggerSchema("Data de criaÃ§Ã£o do projeto.")]
    public DateTime CreatedAt { get; set; }
    
}

