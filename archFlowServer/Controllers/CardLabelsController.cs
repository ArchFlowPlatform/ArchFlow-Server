using archFlowServer.Models.Dtos.Cards.Labels;
using archFlowServer.Models.ViewModels;
using archFlowServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace archFlowServer.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/cards/{cardId:int}/labels")]
public class CardLabelsController : ControllerBase
{
    private readonly CardLabelService _service;

    public CardLabelsController(CardLabelService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromRoute] Guid projectId, [FromRoute] int cardId)
    {
        var data = await _service.GetAllAsync(projectId, cardId);
        return Ok(ResultViewModel.Ok("Labels do card carregadas com sucesso.", data));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status201Created)]
    public async Task<IActionResult> Add(
        [FromRoute] Guid projectId,
        [FromRoute] int cardId,
        [FromBody] AddCardLabelDto dto)
    {
        var created = await _service.AddAsync(projectId, cardId, dto);
        return StatusCode(StatusCodes.Status201Created,
            ResultViewModel.Ok("Label vinculada ao card com sucesso.", created));
    }

    [HttpDelete("{cardLabelId:int}")]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Remove(
        [FromRoute] Guid projectId,
        [FromRoute] int cardId,
        [FromRoute] int cardLabelId)
    {
        await _service.RemoveAsync(projectId, cardId, cardLabelId);
        return Ok(ResultViewModel.Ok("Label desvinculada do card com sucesso."));
    }
}