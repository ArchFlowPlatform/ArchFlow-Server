using archFlowServer.Models.Dtos.Cards.Attachments;
using archFlowServer.Models.ViewModels;
using archFlowServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace archFlowServer.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/cards/{cardId:int}/attachments")]
public class CardAttachmentsController : ControllerBase
{
    private readonly CardAttachmentService _service;

    public CardAttachmentsController(CardAttachmentService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromRoute] Guid projectId, [FromRoute] int cardId)
    {
        var data = await _service.GetAllAsync(projectId, cardId);
        return Ok(ResultViewModel.Ok("Anexos do card carregados com sucesso.", data));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromRoute] Guid projectId,
        [FromRoute] int cardId,
        [FromBody] CreateCardAttachmentDto dto)
    {
        var created = await _service.CreateAsync(projectId, cardId, dto);
        return StatusCode(StatusCodes.Status201Created,
            ResultViewModel.Ok("Anexo criado com sucesso.", created));
    }

    [HttpDelete("{attachmentId:int}")]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid projectId,
        [FromRoute] int cardId,
        [FromRoute] int attachmentId)
    {
        await _service.DeleteAsync(projectId, cardId, attachmentId);
        return Ok(ResultViewModel.Ok("Anexo removido com sucesso."));
    }
}