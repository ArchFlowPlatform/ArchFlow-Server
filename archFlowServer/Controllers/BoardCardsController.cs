using archFlowServer.Models.Dtos.Board.Cards;
using archFlowServer.Models.ViewModels;
using archFlowServer.Services;
using ArchFlowServer.Models.Dtos.Board.Cards;
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
    [SwaggerOperation(Summary = "Lista cards da coluna (ordenados por position).")]
    [ProducesResponseType(typeof(ResultViewModel<IEnumerable<BoardCardResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromRoute] Guid projectId, [FromRoute] Guid sprintId, [FromRoute] int columnId)
    {
        var cards = await _service.GetAllByColumnAsync(projectId, sprintId, columnId);
        return Ok(ResultViewModel<IEnumerable<BoardCardResponseDto>>.Ok("Cards encontrados com sucesso.", cards));
    }

    [HttpPost]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Cria card na coluna (representa uma UserStory).")]
    [ProducesResponseType(typeof(ResultViewModel<BoardCardResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromRoute] Guid projectId,
        [FromRoute] Guid sprintId,
        [FromRoute] int columnId,
        [FromBody] CreateBoardCardDto dto)
    {
        var created = await _service.CreateAsync(projectId, sprintId, columnId, dto);

        return CreatedAtAction(
            nameof(GetAll),
            new { projectId, sprintId, columnId },
            ResultViewModel<BoardCardResponseDto>.Ok("Card criado com sucesso.", created)
        );
    }

    // reorder dentro da coluna (drag)
    [HttpPatch("{cardId:int}/reorder")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Reordena card dentro da mesma coluna (drag).")]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reorder(
        [FromRoute] Guid projectId,
        [FromRoute] Guid sprintId,
        [FromRoute] int columnId,
        [FromRoute] int cardId,
        [FromBody] ReorderBoardCardDto dto)
    {
        dto.CardId = cardId;

        await _service.ReorderAsync(projectId, sprintId, columnId, dto);
        return Ok(ResultViewModel.Ok("Card reordenado com sucesso."));
    }

    // move entre colunas (drag)
    [HttpPatch("{cardId:int}/move")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Move card para outra coluna (drag).")]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Move(
        [FromRoute] Guid projectId,
        [FromRoute] Guid sprintId,
        [FromRoute] int columnId,
        [FromRoute] int cardId,
        [FromBody] MoveBoardCardDto dto)
    {
        // columnId na rota é a coluna atual "do contexto UI", mas a fonte real é o próprio card.
        await _service.MoveAsync(projectId, sprintId, cardId, dto);
        return Ok(ResultViewModel.Ok("Card movido com sucesso."));
    }

    [HttpDelete("{cardId:int}")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Remove card (reindexa positions da coluna).")]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid projectId,
        [FromRoute] Guid sprintId,
        [FromRoute] int columnId,
        [FromRoute] int cardId)
    {
        await _service.DeleteAsync(projectId, sprintId, cardId);
        return Ok(ResultViewModel.Ok("Card removido com sucesso."));
    }
}