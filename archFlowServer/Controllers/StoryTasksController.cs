using archFlowServer.Models.Dtos.Task;
using archFlowServer.Models.ViewModels;
using archFlowServer.Services;
using ArchFlowServer.Models.Dtos.Task;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId:guid}/sprints/{sprintId:guid}/items/{sprintItemId:int}/tasks")]
public class StoryTasksController : ControllerBase
{
    private readonly StoryTaskService _service;

    public StoryTasksController(StoryTaskService service) => _service = service;

    [HttpGet]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Lista tasks do sprint item (ordenadas por position).")]
    [ProducesResponseType(typeof(ResultViewModel<IEnumerable<StoryTaskResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromRoute] Guid projectId, [FromRoute] Guid sprintId, [FromRoute] int sprintItemId)
    {
        var tasks = await _service.GetAllAsync(projectId, sprintId, sprintItemId);
        return Ok(ResultViewModel<IEnumerable<StoryTaskResponseDto>>.Ok("Tasks encontradas com sucesso.", tasks));
    }

    [HttpPost]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Cria task no sprint item (position é definido automaticamente).")]
    [ProducesResponseType(typeof(ResultViewModel<StoryTaskResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromRoute] Guid projectId,
        [FromRoute] Guid sprintId,
        [FromRoute] int sprintItemId,
        [FromBody] CreateStoryTaskDto dto)
    {
        var created = await _service.CreateAsync(projectId, sprintId, sprintItemId, dto);

        return CreatedAtAction(
            nameof(GetAll),
            new { projectId, sprintId, sprintItemId },
            ResultViewModel<StoryTaskResponseDto>.Ok("Task criada com sucesso.", created)
        );
    }

    [HttpPut("{taskId:int}")]
    [Authorize(Policy = "CanViewProject")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid projectId,
        [FromRoute] Guid sprintId,
        [FromRoute] int sprintItemId,
        [FromRoute] int taskId,
        [FromBody] UpdateStoryTaskDto dto)
    {
        var updated = await _service.UpdateAsync(projectId, sprintId, sprintItemId, taskId, dto);
        return Ok(ResultViewModel<StoryTaskResponseDto>.Ok("Task atualizada com sucesso.", updated));
    }

    [HttpDelete("{taskId:int}")]
    [Authorize(Policy = "CanViewProject")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid projectId,
        [FromRoute] Guid sprintId,
        [FromRoute] int sprintItemId,
        [FromRoute] int taskId)
    {
        await _service.DeleteAsync(projectId, sprintId, sprintItemId, taskId);
        return Ok(ResultViewModel.Ok("Task removida com sucesso."));
    }

    [HttpPatch("{taskId:int}/reorder")]
    [Authorize(Policy = "CanViewProject")]
    public async Task<IActionResult> Reorder(
        [FromRoute] Guid projectId,
        [FromRoute] Guid sprintId,
        [FromRoute] int sprintItemId,
        [FromRoute] int taskId,
        [FromBody] ReorderStoryTaskDto dto)
    {
        dto.TaskId = taskId;
        await _service.ReorderAsync(projectId, sprintId, sprintItemId, dto);
        return Ok(ResultViewModel.Ok("Task reordenada com sucesso."));
    }

    [HttpPatch("{taskId:int}/move")]
    [Authorize(Policy = "CanViewProject")]
    public async Task<IActionResult> Move(
        [FromRoute] Guid projectId,
        [FromRoute] Guid sprintId,
        [FromRoute] int sprintItemId,
        [FromRoute] int taskId,
        [FromBody] MoveStoryTaskDto dto)
    {
        dto.TaskId = taskId;
        await _service.MoveAsync(projectId, sprintId, fromSprintItemId: sprintItemId, dto);
        return Ok(ResultViewModel.Ok("Task movida com sucesso."));
    }
}