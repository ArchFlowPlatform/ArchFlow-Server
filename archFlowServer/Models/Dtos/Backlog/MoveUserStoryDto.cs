using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Models.Dtos.Backlog;

public class MoveUserStoryDto
{
    [SwaggerSchema("Id da user story que será movida.")]
    public int StoryId { get; set; }

    [SwaggerSchema("EpicId de destino.")]
    public int ToEpicId { get; set; }

    [SwaggerSchema("Posição desejada no backlog do epic destino (0-based recomendado).")]
    public int ToPosition { get; set; }
}