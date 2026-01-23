using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Models.Dtos.Project;

[SwaggerSchema(Description = "DTO para criaÃ§Ã£o de um projeto.")]
public class CreateProjectDto
{
    [SwaggerSchema("Nome do projeto.")]
    [Required(ErrorMessage = "Nome Ã© obrigatÃ³rio.")]
    [MinLength(3, ErrorMessage = "Nome deve ter no mÃ­nimo 3 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [SwaggerSchema("DescriÃ§Ã£o do projeto.")]
    public string Description { get; set; } = string.Empty;
}

