using archFlowServer.Models.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Models.Dtos.Project;

public class ProjectMemberResponseDto
{
    [SwaggerSchema("Id do usuário.")]
    public Guid UserId { get; set; }

    [SwaggerSchema("Nome completo do usuário.")]
    public string Name { get; set; } = string.Empty;

    [SwaggerSchema("Email do usuário.")]
    public string Email { get; set; } = string.Empty;

    [SwaggerSchema("Função do membro na equipe (exemplo: Owner, ScrumMaster, ProductOwner, Developer).")]
    public MemberRole Role { get; set; } = MemberRole.Developer;

    [SwaggerSchema("Data de entrada do membro no projeto.")]
    public DateTime JoinedAt { get; set; }
}
