using archFlowServer.Models.Dtos.Sprint;
using archFlowServer.Models.ViewModels;
using archFlowServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId:guid}/sprints/{sprintId:guid}/items")]
public class SprintItemsController : ControllerBase
{
    private readonly SprintItemService _service;

    public SprintItemsController(SprintItemService service)
    {
        _service = service;
    }

    /// <summary>Lista itens do sprint backlog.</summary>
    [HttpGet]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Lista itens do sprint backlog.",
        Description = "Retorna os itens do sprint backlog ordenados por Position."
    )]
    [ProducesResponseType(typeof(ResultViewModel<IEnumerable<SprintItemResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll(
        [FromRoute] Guid projectId,
        [FromRoute] Guid sprintId)
    {
        var items = await _service.GetAllAsync(projectId, sprintId);

        return Ok(ResultViewModel<IEnumerable<SprintItemResponseDto>>.Ok(
            "Itens do sprint backlog encontrados com sucesso.",
            items
        ));
    }

    /// <summary>Busca um item do sprint backlog pelo ID.</summary>
    [HttpGet("{itemId:int}")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Busca um item do sprint backlog.",
        Description = "Retorna um item específico do sprint backlog."
    )]
    [ProducesResponseType(typeof(ResultViewModel<SprintItemResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid projectId,
        [FromRoute] Guid sprintId,
        [FromRoute] int itemId)
    {
        var item = await _service.GetByIdAsync(projectId, sprintId, itemId);

        return Ok(ResultViewModel<SprintItemResponseDto>.Ok(
            "Item encontrado com sucesso.",
            item
        ));
    }

    /// <summary>Adiciona uma user story ao sprint backlog.</summary>
    [HttpPost]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Adiciona um item ao sprint backlog.",
        Description = "Adiciona uma user story ao sprint. Se Position não for informada, adiciona no final."
    )]
    [ProducesResponseType(typeof(ResultViewModel<SprintItemResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        [FromRoute] Guid projectId,
        [FromRoute] Guid sprintId,
        [FromBody] CreateSprintItemDto dto)
    {
        var created = await _service.CreateAsync(projectId, sprintId, dto);

        return CreatedAtAction(
            nameof(GetById),
            new { projectId, sprintId, itemId = created.Id },
            ResultViewModel<SprintItemResponseDto>.Ok("Item adicionado ao sprint backlog com sucesso.", created)
        );
    }

    /// <summary>Atualiza posição/notas do item no sprint backlog.</summary>
    [HttpPatch("{itemId:int}")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Atualiza item do sprint backlog.",
        Description = "Permite reordenar (Position) e atualizar Notes."
    )]
    [ProducesResponseType(typeof(ResultViewModel<SprintItemResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid projectId,
        [FromRoute] Guid sprintId,
        [FromRoute] int itemId,
        [FromBody] UpdateSprintItemDto dto)
    {
        var updated = await _service.UpdateAsync(projectId, sprintId, itemId, dto);

        return Ok(ResultViewModel<SprintItemResponseDto>.Ok(
            "Item atualizado com sucesso.",
            updated
        ));
    }

    /// <summary>Remove um item do sprint backlog.</summary>
    [HttpDelete("{itemId:int}")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Remove item do sprint backlog.",
        Description = "Remove o item e ajusta a ordenação (fecha o buraco) dentro do sprint."
    )]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid projectId,
        [FromRoute] Guid sprintId,
        [FromRoute] int itemId)
    {
        await _service.DeleteAsync(projectId, sprintId, itemId);
        return Ok(ResultViewModel.Ok("Item removido do sprint backlog com sucesso."));
    }
}
