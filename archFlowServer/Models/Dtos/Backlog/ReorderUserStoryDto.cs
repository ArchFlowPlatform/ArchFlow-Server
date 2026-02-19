using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Models.Dtos.Backlog;

public class ReorderUserStoryDto
{
    [SwaggerSchema("Id da user story que será reordenada.")]
    public int StoryId { get; set; }

    [SwaggerSchema("Nova posição no backlog dentro do epic (0-based recomendado).")]
    public int ToPosition { get; set; }
}


