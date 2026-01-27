using archFlowServer.Models.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace ArchFlowServer.Models.Dtos.Project
{
    [SwaggerSchema(Description = "DTO retornado ao consultar convites.")]
    public class ProjectInviteResponseDto
    {
        [SwaggerSchema("Identificador unico do convite.")]
        public Guid Id { get; set; }

        [SwaggerSchema("Identificador unico do projeto.")]
        public Guid ProjectId { get; set; }

        [SwaggerSchema("Email para enviar o convite.")]
        public string Email { get; set; } = string.Empty;

        [SwaggerSchema("Função do membro na equipe (exemplo: 'Owner', 'ScrumMaster', 'ProductOwner', 'Developer').")]
        public MemberRole Role { get; set; }
        
        [SwaggerSchema("Token do convite.")]
        public string Token { get; set; } = string.Empty;

        [SwaggerSchema("Data de expiração do convite.")]
        public DateTime ExpiresAt { get; set; }

        [SwaggerSchema("Data de envio do convite.")]
        public DateTime CreatedAt { get; set; }

        [SwaggerSchema("Status do convite.")]
        public InviteStatus Status { get; set; }
    }
}
