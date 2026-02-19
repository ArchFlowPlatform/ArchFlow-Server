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
[Route("api/projects/{projectId:guid}/user-stories/{userStoryId:int}/tasks")]
public class StoryTasksController : ControllerBase
{
    private readonly StoryTaskService _service;

    public StoryTasksController(StoryTaskService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Lista tasks da user story (ordenadas por position).")]
    [ProducesResponseType(typeof(ResultViewModel<IEnumerable<StoryTaskResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromRoute] Guid projectId, [FromRoute] int userStoryId)
    {
        var tasks = await _service.GetAllAsync(projectId, userStoryId);
        return Ok(ResultViewModel<IEnumerable<StoryTaskResponseDto>>.Ok("Tasks encontradas com sucesso.", tasks));
    }

    [HttpPost]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Cria task na user story (position é definido automaticamente).")]
    [ProducesResponseType(typeof(ResultViewModel<StoryTaskResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromRoute] Guid projectId,
        [FromRoute] int userStoryId,
        [FromBody] CreateStoryTaskDto dto)
    {
        var created = await _service.CreateAsync(projectId, userStoryId, dto);

        // retorna Location apontando para o GET da lista do container
        return CreatedAtAction(
            nameof(GetAll),
            new { projectId, userStoryId },
            ResultViewModel<StoryTaskResponseDto>.Ok("Task criada com sucesso.", created)
        );
    }

    [HttpPut("{taskId:int}")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Atualiza task da user story.")]
    [ProducesResponseType(typeof(ResultViewModel<StoryTaskResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid projectId,
        [FromRoute] int userStoryId,
        [FromRoute] int taskId,
        [FromBody] UpdateStoryTaskDto dto)
    {
        var updated = await _service.UpdateAsync(projectId, userStoryId, taskId, dto);
        return Ok(ResultViewModel<StoryTaskResponseDto>.Ok("Task atualizada com sucesso.", updated));
    }

    [HttpDelete("{taskId:int}")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Remove task da user story (reindexa positions).")]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid projectId,
        [FromRoute] int userStoryId,
        [FromRoute] int taskId)
    {
        await _service.DeleteAsync(projectId, userStoryId, taskId);
        return Ok(ResultViewModel.Ok("Task removida com sucesso."));
    }

    // ==========================
    // Drag & Drop ordering
    // ==========================

    [HttpPatch("{taskId:int}/reorder")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Reordena task dentro da mesma user story (drag).")]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reorder(
        [FromRoute] Guid projectId,
        [FromRoute] int userStoryId,
        [FromRoute] int taskId,
        [FromBody] ReorderStoryTaskDto dto)
    {
        // garante consistência: o id do body deve bater com a rota
        dto.TaskId = taskId;

        await _service.ReorderAsync(projectId, userStoryId, dto);
        return Ok(ResultViewModel.Ok("Task reordenada com sucesso."));
    }

    [HttpPatch("{taskId:int}/move")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Move task para outra user story (drag) e define position no destino.")]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Move(
        [FromRoute] Guid projectId,
        [FromRoute] int userStoryId,
        [FromRoute] int taskId,
        [FromBody] MoveStoryTaskDto dto)
    {
        // garante consistência: o id do body deve bater com a rota
        dto.TaskId = taskId;

        await _service.MoveAsync(projectId, fromUserStoryId: userStoryId, dto);
        return Ok(ResultViewModel.Ok("Task movida com sucesso."));
    }
}