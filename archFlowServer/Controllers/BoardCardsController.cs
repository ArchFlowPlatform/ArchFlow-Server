using archFlowServer.Models.Dtos.Board.Cards;
using archFlowServer.Models.ViewModels;
using archFlowServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId:guid}/sprints/{sprintId:guid}/board/columns/{columnId:int}/cards")]
public class BoardCardsController : ControllerBase
{
    private readonly BoardCardService _service;

    public BoardCardsController(BoardCardService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Lista cards da coluna.")]
    public async Task<IActionResult> GetAll(Guid projectId, Guid sprintId, int columnId)
    {
        var cards = await _service.GetAllByColumnAsync(projectId, sprintId, columnId);
        return Ok(ResultViewModel<IEnumerable<BoardCardResponseDto>>.Ok("Cards encontrados com sucesso.", cards));
    }

    [HttpPost]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Cria card na coluna.")]
    public async Task<IActionResult> Create(Guid projectId, Guid sprintId, int columnId, [FromBody] CreateBoardCardDto dto)
    {
        var created = await _service.CreateAsync(projectId, sprintId, columnId, dto);
        return Ok(ResultViewModel<BoardCardResponseDto>.Ok("Card criado com sucesso.", created));
    }

    [HttpPut("{cardId:int}")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Atualiza card.")]
    public async Task<IActionResult> Update(Guid projectId, Guid sprintId, int columnId, int cardId, [FromBody] UpdateBoardCardDto dto)
    {
        var updated = await _service.UpdateAsync(projectId, sprintId, cardId, dto);
        return Ok(ResultViewModel<BoardCardResponseDto>.Ok("Card atualizado com sucesso.", updated));
    }

    [HttpPost("{cardId:int}/move")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Move card entre colunas.")]
    public async Task<IActionResult> Move(Guid projectId, Guid sprintId, int columnId, int cardId, [FromBody] MoveBoardCardDto dto)
    {
        await _service.MoveAsync(projectId, sprintId, cardId, dto);
        return Ok(ResultViewModel.Ok("Card movido com sucesso."));
    }

    [HttpDelete("{cardId:int}")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Remove card.")]
    public async Task<IActionResult> Delete(Guid projectId, Guid sprintId, int columnId, int cardId)
    {
        await _service.DeleteAsync(projectId, sprintId, cardId);
        return Ok(ResultViewModel.Ok("Card removido com sucesso."));
    }
}
