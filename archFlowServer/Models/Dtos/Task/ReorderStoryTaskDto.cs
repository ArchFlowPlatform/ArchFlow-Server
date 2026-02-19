using Swashbuckle.AspNetCore.Annotations;

namespace ArchFlowServer.Models.Dtos.Task
{
    public class ReorderStoryTaskDto
    {
        [SwaggerSchema("Id da task.")]
        public int TaskId { get; set; }

        [SwaggerSchema("Nova posição (0-based) dentro da mesma user story.")]
        public int ToPosition { get; set; }
    }
}
