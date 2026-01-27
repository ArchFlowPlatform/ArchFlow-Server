using archFlowServer.Models.Dtos.Backlog;
using archFlowServer.Models.ViewModels;
using archFlowServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId:guid}/backlog")]
public class BacklogController : ControllerBase
{
    private readonly BacklogService _service;

    public BacklogController(BacklogService service)
    {
        _service = service;
    }

    // ============================
    // EPICS
    // ============================

    /// <summary>Cria um épico no product backlog do projeto.</summary>
    [HttpPost("epics")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Cria um épico no product backlog do projeto.",
        Description = "Cria um novo épico no backlog do projeto. A posição (Position) é calculada no backend (append)."
    )]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateEpic(
        [FromRoute] Guid projectId,
        [FromBody] CreateEpicDto dto)
    {
        await _service.CreateEpicAsync(projectId, dto);
        return Ok(ResultViewModel.Ok("Épico criado com sucesso."));
    }

    /// <summary>Atualiza campos do épico.</summary>
    [HttpPatch("epics/{epicId:int}")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Atualiza um épico.",
        Description = "Atualiza parcialmente um épico do projeto. Valida se o épico pertence ao backlog do projeto."
    )]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEpic(
        [FromRoute] Guid projectId,
        [FromRoute] int epicId,
        [FromBody] UpdateEpicDto dto)
    {
        await _service.UpdateEpicAsync(projectId, epicId, dto);
        return Ok(ResultViewModel.Ok("Épico atualizado com sucesso."));
    }

    /// <summary>Reordena um épico dentro do backlog do projeto.</summary>
    [HttpPatch("epics/reorder")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Reordena um épico.",
        Description = "Move um épico da posição atual para a posição alvo (ToPosition), ajustando a faixa intermediária."
    )]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderEpic(
        [FromRoute] Guid projectId,
        [FromBody] ReorderEpicDto dto)
    {
        await _service.ReorderEpicAsync(projectId, dto);
        return Ok(ResultViewModel.Ok("Épico reordenado com sucesso."));
    }

    /// <summary>Arquiva um épico e todas as user stories dele.</summary>
    [HttpPatch("epics/{epicId:int}/archive")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Arquiva um épico.",
        Description = "Arquiva o épico e arquiva em cascata todas as user stories do épico (operações transacionais)."
    )]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ArchiveEpic(
        [FromRoute] Guid projectId,
        [FromRoute] int epicId)
    {
        await _service.ArchiveEpicAsync(projectId, epicId);
        return Ok(ResultViewModel.Ok("Épico arquivado com sucesso."));
    }

    /// <summary>Restaura um épico e todas as user stories dele.</summary>
    [HttpPatch("epics/{epicId:int}/restore")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Restaura um épico arquivado.",
        Description = "Restaura o épico e restaura em cascata todas as user stories do épico (operações transacionais)."
    )]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreEpic(
        [FromRoute] Guid projectId,
        [FromRoute] int epicId)
    {
        await _service.RestoreEpicAsync(projectId, epicId);
        return Ok(ResultViewModel.Ok("Épico restaurado com sucesso."));
    }

    // ============================
    // USER STORIES
    // ============================

    /// <summary>Cria uma user story dentro de um épico do projeto.</summary>
    [HttpPost("epics/{epicId:int}/stories")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Cria uma user story.",
        Description = "Cria uma user story dentro do épico informado. A posição (Position) é calculada no backend (append)."
    )]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateStory(
        [FromRoute] Guid projectId,
        [FromRoute] int epicId,
        [FromBody] CreateUserStoryDto dto)
    {
        await _service.CreateUserStoryAsync(projectId, epicId, dto);
        return Ok(ResultViewModel.Ok("User story criada com sucesso."));
    }

    /// <summary>Atualiza campos da user story.</summary>
    [HttpPatch("stories/{storyId:int}")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Atualiza uma user story.",
        Description = "Atualiza parcialmente uma user story do projeto. Valida se a story pertence ao backlog do projeto."
    )]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStory(
        [FromRoute] Guid projectId,
        [FromRoute] int storyId,
        [FromBody] UpdateUserStoryDto dto)
    {
        await _service.UpdateUserStoryAsync(projectId, storyId, dto);
        return Ok(ResultViewModel.Ok("User story atualizada com sucesso."));
    }

    /// <summary>Reordena uma user story dentro do mesmo épico.</summary>
    [HttpPatch("stories/reorder")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Reordena uma user story.",
        Description = "Move uma user story para outra posição dentro do mesmo épico, ajustando a faixa intermediária."
    )]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderStory(
        [FromRoute] Guid projectId,
        [FromBody] ReorderUserStoryDto dto)
    {
        await _service.ReorderUserStoryAsync(projectId, dto);
        return Ok(ResultViewModel.Ok("User story reordenada com sucesso."));
    }

    /// <summary>Move uma user story entre épicos (e define a posição de destino).</summary>
    [HttpPatch("stories/move")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Move uma user story entre épicos.",
        Description = "Move uma user story do épico atual para outro épico, ajustando posições na origem e no destino (transacional)."
    )]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MoveStory(
        [FromRoute] Guid projectId,
        [FromBody] MoveUserStoryDto dto)
    {
        await _service.MoveUserStoryAsync(projectId, dto);
        return Ok(ResultViewModel.Ok("User story movida com sucesso."));
    }

    /// <summary>Arquiva uma user story.</summary>
    [HttpPatch("stories/{storyId:int}/archive")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Arquiva uma user story.",
        Description = "Arquiva a user story informada. Valida se ela pertence ao backlog do projeto."
    )]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ArchiveStory(
        [FromRoute] Guid projectId,
        [FromRoute] int storyId)
    {
        await _service.ArchiveUserStoryAsync(projectId, storyId);
        return Ok(ResultViewModel.Ok("User story arquivada com sucesso."));
    }

    /// <summary>Restaura uma user story arquivada.</summary>
    [HttpPatch("stories/{storyId:int}/restore")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Restaura uma user story arquivada.",
        Description = "Restaura a user story informada. Valida se ela pertence ao backlog do projeto."
    )]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreStory(
        [FromRoute] Guid projectId,
        [FromRoute] int storyId)
    {
        await _service.RestoreUserStoryAsync(projectId, storyId);
        return Ok(ResultViewModel.Ok("User story restaurada com sucesso."));
    }

    // ============================
    // BACKLOG (READ/UPDATE)
    // ============================

    /// <summary>Retorna o product backlog completo (overview + epics + user stories).</summary>
    [HttpGet]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Retorna o product backlog completo.",
        Description = "Retorna overview, épicos e user stories (ordenados por Position)."
    )]
    [ProducesResponseType(typeof(ResultViewModel<ProductBacklogResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBacklog([FromRoute] Guid projectId)
    {
        var dto = await _service.GetBacklogByProjectIdAsync(projectId);

        return Ok(
            ResultViewModel<ProductBacklogResponseDto>.Ok(
                "Backlog carregado com sucesso.",
                dto
            )
        );
    }

    /// <summary>Atualiza o overview do backlog.</summary>
    [HttpPatch]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Atualiza o overview do backlog.",
        Description = "Atualiza o campo Overview do product backlog do projeto."
    )]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOverview(
        [FromRoute] Guid projectId,
        [FromBody] UpdateBacklogOverviewDto dto)
    {
        await _service.UpdateOverviewAsync(projectId, dto);
        return Ok(ResultViewModel.Ok("Overview atualizado com sucesso."));
    }
}
