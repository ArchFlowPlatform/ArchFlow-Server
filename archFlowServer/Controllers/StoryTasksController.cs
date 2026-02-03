using archFlowServer.Models.Dtos.Task;
using archFlowServer.Models.ViewModels;
using archFlowServer.Services;
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
    [SwaggerOperation(Summary = "Lista tasks da user story.")]
    public async Task<IActionResult> GetAll(Guid projectId, int userStoryId)
    {
        var tasks = await _service.GetAllAsync(projectId, userStoryId);
        return Ok(ResultViewModel<IEnumerable<StoryTaskResponseDto>>.Ok("Tasks encontradas com sucesso.", tasks));
    }

    [HttpPost]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Cria task na user story.")]
    public async Task<IActionResult> Create(Guid projectId, int userStoryId, [FromBody] CreateStoryTaskDto dto)
    {
        var created = await _service.CreateAsync(projectId, userStoryId, dto);
        return CreatedAtAction(nameof(GetAll), new { projectId, userStoryId }, ResultViewModel<StoryTaskResponseDto>.Ok("Task criada com sucesso.", created));
    }

    [HttpPut("{taskId:int}")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Atualiza task da user story.")]
    public async Task<IActionResult> Update(Guid projectId, int userStoryId, int taskId, [FromBody] UpdateStoryTaskDto dto)
    {
        var updated = await _service.UpdateAsync(projectId, userStoryId, taskId, dto);
        return Ok(ResultViewModel<StoryTaskResponseDto>.Ok("Task atualizada com sucesso.", updated));
    }

    [HttpDelete("{taskId:int}")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Remove task da user story.")]
    public async Task<IActionResult> Delete(Guid projectId, int userStoryId, int taskId)
    {
        await _service.DeleteAsync(projectId, userStoryId, taskId);
        return Ok(ResultViewModel.Ok("Task removida com sucesso."));
    }
}
