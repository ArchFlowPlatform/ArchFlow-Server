using archFlowServer.Models.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Models.Dtos.Backlog;

public class UpdateUserStoryDto
{
    [SwaggerSchema("Título da user story.")]
    public string? Title { get; set; }

    [SwaggerSchema("Persona (ex: 'Como usuário').")]
    public string? Persona { get; set; }

    [SwaggerSchema("Descrição.")]
    public string? Description { get; set; }

    [SwaggerSchema("Critérios de aceitação.")]
    public string? AcceptanceCriteria { get; set; }

    [SwaggerSchema("Complexidade.")]
    public UserStoryComplexity? Complexity { get; set; }

    [SwaggerSchema("Effort (ex: story points).")]
    public int? Effort { get; set; }

    [SwaggerSchema("Dependências.")]
    public string? Dependencies { get; set; }

    [SwaggerSchema("Prioridade.")]
    public int? Priority { get; set; }

    [SwaggerSchema("Valor de negócio.")]
    public BusinessValue? BusinessValue { get; set; }

    [SwaggerSchema("Status.")]
    public UserStoryStatus? Status { get; set; }

    [SwaggerSchema("Posição no backlog (dentro do épico).")]
    public int? BacklogPosition { get; set; } // ✅

    [SwaggerSchema("Responsável (Guid).")]
    public Guid? AssigneeId { get; set; }
}
