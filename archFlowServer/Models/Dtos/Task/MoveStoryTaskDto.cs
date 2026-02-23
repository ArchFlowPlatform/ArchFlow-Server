using Swashbuckle.AspNetCore.Annotations;

namespace ArchFlowServer.Models.Dtos.Task
{
    public class MoveStoryTaskDto
    {
        [SwaggerSchema("Id da task.")]
        public int TaskId { get; set; }

        [SwaggerSchema("SprintItem de destino.")]
        public int ToSprintItemId { get; set; }

        [SwaggerSchema("Nova posição (0-based) dentro da user story de destino.")]
        public int ToPosition { get; set; }
    }
}


