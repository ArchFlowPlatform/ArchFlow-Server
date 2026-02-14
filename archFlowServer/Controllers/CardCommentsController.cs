using archFlowServer.Models.Dtos.Cards.Comments;
using archFlowServer.Models.ViewModels;
using archFlowServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace archFlowServer.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/cards/{cardId:int}/comments")]
public class CardCommentsController : ControllerBase
{
    private readonly CardCommentService _service;

    public CardCommentsController(CardCommentService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromRoute] Guid projectId, [FromRoute] int cardId)
    {
        var data = await _service.GetAllAsync(projectId, cardId);
        return Ok(ResultViewModel.Ok("Comentários carregados com sucesso.", data));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromRoute] Guid projectId, [FromRoute] int cardId, [FromBody] CreateCardCommentDto dto)
    {
        var created = await _service.CreateAsync(projectId, cardId, dto);
        return StatusCode(StatusCodes.Status201Created, ResultViewModel.Ok("Comentário criado com sucesso.", created));
    }

    [HttpPut("{commentId:int}")]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid projectId,
        [FromRoute] int cardId,
        [FromRoute] int commentId,
        [FromBody] UpdateCardCommentDto dto)
    {
        var updated = await _service.UpdateAsync(projectId, cardId, commentId, dto);
        return Ok(ResultViewModel.Ok("Comentário atualizado com sucesso.", updated));
    }

    [HttpDelete("{commentId:int}")]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete([FromRoute] Guid projectId, [FromRoute] int cardId, [FromRoute] int commentId)
    {
        await _service.DeleteAsync(projectId, cardId, commentId);
        return Ok(ResultViewModel.Ok("Comentário removido com sucesso."));
    }
}
