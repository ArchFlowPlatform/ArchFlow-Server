using archFlowServer.Models.Dtos.Board;
using archFlowServer.Models.ViewModels;
using archFlowServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId:guid}/sprints/{sprintId:guid}/board")]
public class BoardsController : ControllerBase
{
    private readonly BoardService _service;

    public BoardsController(BoardService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Busca o board da sprint.",
        Description = "Retorna o board 1:1 associado à sprint."
    )]
    [ProducesResponseType(typeof(ResultViewModel<BoardResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(
        [FromRoute] Guid projectId,
        [FromRoute] Guid sprintId)
    {
        var board = await _service.GetBySprintAsync(projectId, sprintId);

        return Ok(ResultViewModel<BoardResponseDto>.Ok(
            "Board encontrado com sucesso.",
            board
        ));
    }

    [HttpPatch]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Atualiza o board da sprint.",
        Description = "Atualiza nome, descrição e tipo do board (1:1)."
    )]
    [ProducesResponseType(typeof(ResultViewModel<BoardResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid projectId,
        [FromRoute] Guid sprintId,
        [FromBody] UpdateBoardDto dto)
    {
        var updated = await _service.UpdateAsync(projectId, sprintId, dto);

        return Ok(ResultViewModel<BoardResponseDto>.Ok(
            "Board atualizado com sucesso.",
            updated
        ));
    }
}
