using System.ComponentModel.DataAnnotations;
using archFlowServer.Models.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Models.Dtos.Project;

[SwaggerSchema(Description = "DTO para adicionar um membro a um projeto.")]
public class AddProjectMemberDto
{
    [SwaggerSchema("ID do usuário que serÃ¡ adicionado ao projeto.")]
    [Required(ErrorMessage = "UserId Ã© obrigatÃ³rio.")]
    public Guid UserId { get; set; }

    [SwaggerSchema("Papel do usuário dentro do projeto.")]
    [Required(ErrorMessage = "Role Ã© obrigatÃ³rio.")]
    public MemberRole Role { get; set; }
}
