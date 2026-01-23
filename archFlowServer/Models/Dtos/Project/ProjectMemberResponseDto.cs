using archFlowServer.Models.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Models.Dtos.Project;

public class ProjectMemberResponseDto
{
    [SwaggerSchema("Id do usuário.")]
    public Guid UserId {get; set;}
    
    [SwaggerSchema("Nome completo do usuário.")]
    public string Name { get; set; } = string.Empty;

    [SwaggerSchema("Email do usuário. Deve ser Ãºnico.")]
    public string Email { get; set; } = string.Empty;
    
    [SwaggerSchema("FunÃ§Ã£o do membro na equipe (exemplo: 'Owner', 'ScrumMaster', 'ProductOwner', 'Developer').")]
    public MemberRole Role { get; set; } = MemberRole.Developer;

    [SwaggerSchema("Senha em texto puro para cadastro. SerÃ¡ hasheada no servidor.")]
    public DateTime JoinedAt { get; set; }
    
   
}
