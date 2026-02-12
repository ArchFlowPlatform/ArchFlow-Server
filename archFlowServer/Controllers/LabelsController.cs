using archFlowServer.Models.Dtos.Labels;
using archFlowServer.Models.ViewModels;
using archFlowServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace archFlowServer.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/labels")]
public class LabelsController : ControllerBase
{
    private readonly LabelService _service;

    public LabelsController(LabelService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromRoute] Guid projectId)
    {
        var data = await _service.GetAllAsync(projectId);
        return Ok(ResultViewModel.Ok("Labels carregadas com sucesso.", data));
    }

    [HttpGet("{labelId:int}")]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById([FromRoute] Guid projectId, [FromRoute] int labelId)
    {
        var data = await _service.GetByIdAsync(projectId, labelId);
        return Ok(ResultViewModel.Ok("Label carregada com sucesso.", data));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromRoute] Guid projectId, [FromBody] CreateLabelDto dto)
    {
        var created = await _service.CreateAsync(projectId, dto);
        return StatusCode(StatusCodes.Status201Created,
            ResultViewModel.Ok("Label criada com sucesso.", created));
    }

    [HttpPut("{labelId:int}")]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid projectId,
        [FromRoute] int labelId,
        [FromBody] UpdateLabelDto dto)
    {
        var updated = await _service.UpdateAsync(projectId, labelId, dto);
        return Ok(ResultViewModel.Ok("Label atualizada com sucesso.", updated));
    }

    [HttpDelete("{labelId:int}")]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete([FromRoute] Guid projectId, [FromRoute] int labelId)
    {
        await _service.DeleteAsync(projectId, labelId);
        return Ok(ResultViewModel.Ok("Label removida com sucesso."));
    }
}
