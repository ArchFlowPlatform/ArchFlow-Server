using archFlowServer.Models.Dtos.Cards.Activities;
using archFlowServer.Models.ViewModels;
using archFlowServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace archFlowServer.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/cards/{cardId:int}/activities")]
public class CardActivitiesController : ControllerBase
{
    private readonly CardActivityService _service;

    public CardActivitiesController(CardActivityService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromRoute] Guid projectId,
        [FromRoute] int cardId,
        [FromQuery] int take = 50)
    {
        var data = await _service.GetAllAsync(projectId, cardId, take);
        return Ok(ResultViewModel.Ok("Atividades carregadas com sucesso.", data));
    }

    // opcional (debug/admin) - pode remover se não quiser criar activity manual
    [HttpPost]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromRoute] Guid projectId,
        [FromRoute] int cardId,
        [FromBody] CreateCardActivityDto dto)
    {
        var created = await _service.CreateAsync(projectId, cardId, dto);
        return StatusCode(StatusCodes.Status201Created, ResultViewModel.Ok("Atividade criada com sucesso.", created));
    }
}