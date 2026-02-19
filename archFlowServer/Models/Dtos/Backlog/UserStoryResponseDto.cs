using archFlowServer.Models.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Models.Dtos.Backlog;

public class UserStoryResponseDto
{
    [SwaggerSchema("Id da user story.")]
    public int Id { get; set; }

    [SwaggerSchema("EpicId.")]
    public int EpicId { get; set; }

    [SwaggerSchema("Título.")]
    public string Title { get; set; } = string.Empty;

    [SwaggerSchema("Persona.")]
    public string Persona { get; set; } = string.Empty;

    [SwaggerSchema("Descrição.")]
    public string Description { get; set; } = string.Empty;

    [SwaggerSchema("Critérios de aceitação.")]
    public string AcceptanceCriteria { get; set; } = string.Empty;

    [SwaggerSchema("Complexidade.")]
    public UserStoryComplexity Complexity { get; set; }

    [SwaggerSchema("Effort.")]
    public int? Effort { get; set; }

    [SwaggerSchema("Posição no backlog (dentro do épico).")]
    public int BacklogPosition { get; set; } // ✅

    [SwaggerSchema("Dependências.")]
    public string Dependencies { get; set; } = string.Empty;

    [SwaggerSchema("Prioridade.")]
    public int Priority { get; set; }

    [SwaggerSchema("Valor de negócio.")]
    public BusinessValue BusinessValue { get; set; }

    [SwaggerSchema("Status.")]
    public UserStoryStatus Status { get; set; }

    [SwaggerSchema("Assignee (Guid).")]
    public Guid? AssigneeId { get; set; }

    [SwaggerSchema("Criado em UTC.")]
    public DateTime CreatedAt { get; set; }

    [SwaggerSchema("Atualizado em UTC.")]
    public DateTime UpdatedAt { get; set; }
}
