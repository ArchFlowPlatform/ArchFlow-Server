using archFlowServer.Models.Dtos.Sprint;
using archFlowServer.Models.ViewModels;
using archFlowServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId:guid}/sprints")]
public class SprintsController : ControllerBase
{
    private readonly SprintService _service;

    public SprintsController(SprintService service)
    {
        _service = service;
    }

    // GET api/projects/{projectId}/sprints
    [HttpGet]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Lista as sprints do projeto.",
        Description = "Retorna as sprints do projeto. Por padrão não inclui arquivadas, a menos que includeArchived=true."
    )]
    [ProducesResponseType(typeof(ResultViewModel<IEnumerable<SprintResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromRoute] Guid projectId,
        [FromQuery] bool includeArchived = false)
    {
        var sprints = await _service.GetAllAsync(projectId, includeArchived);

        return Ok(ResultViewModel<IEnumerable<SprintResponseDto>>.Ok(
            "Sprints carregadas com sucesso.",
            sprints
        ));
    }

    // GET api/projects/{projectId}/sprints/{sprintId}
    [HttpGet("{sprintId:guid}")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Busca uma sprint por ID.",
        Description = "Busca uma sprint do projeto pelo ID. Se includeArchived=false, não retorna sprints arquivadas."
    )]
    [ProducesResponseType(typeof(ResultViewModel<SprintResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid projectId,
        [FromRoute] Guid sprintId,
        [FromQuery] bool includeArchived = false)
    {
        var sprint = await _service.GetByIdAsync(projectId, sprintId, includeArchived);

        return Ok(ResultViewModel<SprintResponseDto>.Ok(
            "Sprint encontrada com sucesso.",
            sprint
        ));
    }

    // POST api/projects/{projectId}/sprints
    [HttpPost]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Cria uma sprint no projeto.",
        Description = "Cria uma sprint. Regra: Board 1:1 por Sprint deve ser criado no mesmo fluxo (service)."
    )]
    [ProducesResponseType(typeof(ResultViewModel<SprintResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromRoute] Guid projectId,
        [FromBody] CreateSprintDto dto)
    {
        var created = await _service.CreateAsync(projectId, dto);

        return CreatedAtAction(
            nameof(GetById),
            new { projectId, sprintId = created.Id },
            ResultViewModel<SprintResponseDto>.Ok("Sprint criada com sucesso.", created)
        );
    }

    // PATCH api/projects/{projectId}/sprints/{sprintId}
    [HttpPatch("{sprintId:guid}")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Atualiza dados da sprint.",
        Description = "Atualiza nome/goal/datas/capacidade. Valida se a sprint pertence ao projeto."
    )]
    [ProducesResponseType(typeof(ResultViewModel<SprintResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid projectId,
        [FromRoute] Guid sprintId,
        [FromBody] UpdateSprintDto dto)
    {
        var updated = await _service.UpdateAsync(projectId, sprintId, dto);

        return Ok(ResultViewModel<SprintResponseDto>.Ok(
            "Sprint atualizada com sucesso.",
            updated
        ));
    }

    // POST api/projects/{projectId}/sprints/{sprintId}/activate
    [HttpPost("{sprintId:guid}/activate")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Ativa uma sprint.",
        Description = "Transição de status planejada → ativa. Deve respeitar regra de no máximo 1 sprint ativa por projeto."
    )]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Activate(
        [FromRoute] Guid projectId,
        [FromRoute] Guid sprintId)
    {
        await _service.ActivateAsync(projectId, sprintId);
        return Ok(ResultViewModel.Ok("Sprint ativada com sucesso."));
    }

    // POST api/projects/{projectId}/sprints/{sprintId}/close
    [HttpPost("{sprintId:guid}/close")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Fecha uma sprint.",
        Description = "Transição de status ativa → fechada."
    )]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Close(
        [FromRoute] Guid projectId,
        [FromRoute] Guid sprintId)
    {
        await _service.CloseAsync(projectId, sprintId);
        return Ok(ResultViewModel.Ok("Sprint fechada com sucesso."));
    }

    // POST api/projects/{projectId}/sprints/{sprintId}/cancel
    [HttpPost("{sprintId:guid}/cancel")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Cancela uma sprint.",
        Description = "Transição para status cancelada. Normalmente não permite cancelar se já estiver fechada."
    )]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(
        [FromRoute] Guid projectId,
        [FromRoute] Guid sprintId)
    {
        await _service.CancelAsync(projectId, sprintId);
        return Ok(ResultViewModel.Ok("Sprint cancelada com sucesso."));
    }

    // PATCH api/projects/{projectId}/sprints/{sprintId}/archive
    [HttpPatch("{sprintId:guid}/archive")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Arquiva uma sprint.",
        Description = "Soft archive da sprint. Não remove do banco."
    )]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Archive(
        [FromRoute] Guid projectId,
        [FromRoute] Guid sprintId)
    {
        await _service.ArchiveAsync(projectId, sprintId);
        return Ok(ResultViewModel.Ok("Sprint arquivada com sucesso."));
    }

    // PATCH api/projects/{projectId}/sprints/{sprintId}/restore
    [HttpPatch("{sprintId:guid}/restore")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(
        Summary = "Restaura uma sprint arquivada.",
        Description = "Soft restore da sprint."
    )]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Restore(
        [FromRoute] Guid projectId,
        [FromRoute] Guid sprintId)
    {
        await _service.RestoreAsync(projectId, sprintId);
        return Ok(ResultViewModel.Ok("Sprint restaurada com sucesso."));
    }
}
