using archFlowServer.Models.ViewModels;
using archFlowServer.Services;
using ArchFlowServer.Models.Dtos.Board.Columns;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId:guid}/sprints/{sprintId:guid}/board/columns")]
public class BoardColumnsController : ControllerBase
{
    private readonly BoardColumnService _service;

    public BoardColumnsController(BoardColumnService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Lista colunas do board da sprint.")]
    public async Task<IActionResult> GetAll(Guid projectId, Guid sprintId)
    {
        var cols = await _service.GetAllAsync(projectId, sprintId);
        return Ok(ResultViewModel<IEnumerable<BoardColumnResponseDto>>.Ok("Colunas encontradas com sucesso.", cols));
    }

    [HttpPost]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Cria coluna no board da sprint.")]
    public async Task<IActionResult> Create(Guid projectId, Guid sprintId, [FromBody] CreateBoardColumnDto dto)
    {
        var created = await _service.CreateAsync(projectId, sprintId, dto);
        return Ok(ResultViewModel<BoardColumnResponseDto>.Ok("Coluna criada com sucesso.", created));
    }

    [HttpPut("{columnId:int}")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Atualiza coluna do board.")]
    public async Task<IActionResult> Update(Guid projectId, Guid sprintId, int columnId, [FromBody] UpdateBoardColumnDto dto)
    {
        var updated = await _service.UpdateAsync(projectId, sprintId, columnId, dto);
        return Ok(ResultViewModel<BoardColumnResponseDto>.Ok("Coluna atualizada com sucesso.", updated));
    }

    [HttpDelete("{columnId:int}")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Remove coluna do board.")]
    public async Task<IActionResult> Delete(Guid projectId, Guid sprintId, int columnId)
    {
        await _service.DeleteAsync(projectId, sprintId, columnId);
        return Ok(ResultViewModel.Ok("Coluna removida com sucesso."));
    }
}
