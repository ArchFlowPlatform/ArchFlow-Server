using archFlowServer.Models.Dtos.Project;
using archFlowServer.Models.Enums;
using archFlowServer.Models.ViewModels;
using archFlowServer.Services;
using archFlowServer.Utils.Authorization;
using archFlowServer.Utils.Extensions;
using ArchFlowServer.Models.Dtos.Project;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly ProjectService _service;

    public ProjectsController(ProjectService service)
    {
        _service = service;
    }

    // GET api/projects
    [HttpGet]
    [SwaggerOperation(Summary = "Lista todos os projetos ativos do usuário logado.")]
    [ProducesResponseType(typeof(ResultViewModel<IEnumerable<ProjectResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.GetUserId();
        var projects = await _service.GetAllAsync(userId);

        return Ok(ResultViewModel<IEnumerable<ProjectResponseDto>>.Ok(
            "Projetos carregados com sucesso.",
            projects
        ));
    }

    // GET api/projects/{id}
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Busca um projeto pelo ID.")]
    [ProducesResponseType(typeof(ResultViewModel<ProjectResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = User.GetUserId();
        var project = await _service.GetByIdAsync(id, userId);

        return Ok(ResultViewModel<ProjectResponseDto>.Ok(
            "Projeto encontrado com sucesso.",
            project
        ));
    }

    // POST api/projects
    [HttpPost]
    [SwaggerOperation(Summary = "Cria um novo projeto.")]
    [ProducesResponseType(typeof(ResultViewModel<ProjectResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto request)
    {
        var userId = User.GetUserId();
        var project = await _service.CreateAsync(request, userId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = project.Id },
            ResultViewModel<ProjectResponseDto>.Ok("Projeto criado com sucesso.", project)
        );
    }

    // PUT api/projects/{id}
    [HttpPut("{id:guid}")]
    [AuthorizeProjectRole(MemberRole.Owner)]
    [SwaggerOperation(Summary = "Atualiza os dados de um projeto.")]
    [ProducesResponseType(typeof(ResultViewModel<ProjectResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectDto dto)
    {
        var userId = User.GetUserId();
        var updated = await _service.UpdateAsync(id, dto, userId);

        return Ok(ResultViewModel<ProjectResponseDto>.Ok(
            "Projeto atualizado com sucesso.",
            updated
        ));
    }

    // POST api/projects/{id}/archive
    [HttpPost("{id:guid}/archive")]
    [Authorize(Policy = "CanArchiveProject")]
    [SwaggerOperation(Summary = "Arquiva um projeto.")]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Archive(Guid id)
    {
        var userId = User.GetUserId();
        await _service.ArchiveAsync(id, userId);

        return Ok(ResultViewModel.Ok("Projeto arquivado com sucesso."));
    }

    // POST api/projects/{id}/restore
    [HttpPost("{id:guid}/restore")]
    [Authorize(Policy = "CanArchiveProject")]
    [SwaggerOperation(Summary = "Restaura um projeto arquivado.")]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Restore(Guid id)
    {
        var userId = User.GetUserId();
        await _service.RestoreAsync(id, userId);

        return Ok(ResultViewModel.Ok("Projeto restaurado com sucesso."));
    }

    // Members

    [HttpPost("{id:guid}/members")]
    [Authorize(Policy = "CanManageMembers")]
    [SwaggerOperation(Summary = "Adiciona um membro ao projeto.")]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddProjectMemberDto dto)
    {
        var executorId = User.GetUserId();

        await _service.AddMemberAsync(
            projectId: id,
            executorUserId: executorId,
            newUserId: dto.UserId,
            role: dto.Role
        );

        return Ok(ResultViewModel.Ok("Membro adicionado com sucesso."));
    }

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    [Authorize(Policy = "CanManageMembers")]
    [SwaggerOperation(Summary = "Remove um membro do projeto.")]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId)
    {
        var executorId = User.GetUserId();

        await _service.RemoveMemberAsync(
            projectId: id,
            executorUserId: executorId,
            userId: userId
        );

        return Ok(ResultViewModel.Ok("Membro removido com sucesso."));
    }

    [HttpGet("{id:guid}/members")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Lista os membros do projeto.")]
    [ProducesResponseType(typeof(ResultViewModel<IEnumerable<ProjectMemberResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMembers(Guid id)
    {
        var userId = User.GetUserId();
        var members = await _service.GetMembersAsync(id, userId);

        return Ok(ResultViewModel<IEnumerable<ProjectMemberResponseDto>>.Ok(
            "Membros carregados com sucesso.",
            members
        ));
    }

    // Invites

    [HttpGet("{id:guid}/invites")]
    [Authorize(Policy = "CanViewProject")]
    [SwaggerOperation(Summary = "Lista os convites do projeto.")]
    [ProducesResponseType(typeof(ResultViewModel<IEnumerable<ProjectInviteResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllProjectInvites(Guid id)
    {
        var userId = User.GetUserId();
        var invites = await _service.GetAllProjectsInviteAsync(id, userId);

        return Ok(ResultViewModel<IEnumerable<ProjectInviteResponseDto>>.Ok(
            "Convites carregados com sucesso.",
            invites
        ));
    }

    [HttpPost("{id:guid}/invites")]
    [Authorize(Policy = "CanManageMembers")]
    [SwaggerOperation(Summary = "Envia convite para participar do projeto.")]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> InviteMember(Guid id, [FromBody] InviteProjectMemberDto dto)
    {
        var userId = User.GetUserId();

        await _service.InviteMemberAsync(
            projectId: id,
            executorUserId: userId,
            email: dto.Email,
            role: dto.Role
        );

        return Ok(ResultViewModel.Ok("Convite enviado com sucesso."));
    }

    // ✅ Agora autenticado (pois service precisa do executorUserId)
    [HttpPut("invites/{token}/accept")]
    [Authorize]
    [SwaggerOperation(Summary = "Aceita um convite (requer autenticação).")]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> AcceptInvite(string token)
    {
        var userId = User.GetUserId();
        await _service.AcceptInviteAsync(token, userId);

        return Ok(ResultViewModel.Ok("Convite aceito com sucesso."));
    }

    [HttpPut("invites/{token}/decline")]
    [Authorize]
    [SwaggerOperation(Summary = "Recusa um convite (requer autenticação).")]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeclineInvite(string token)
    {
        await _service.DeclineInviteAsync(token);

        return Ok(ResultViewModel.Ok("Convite recusado com sucesso."));
    }

    [HttpPut("invites/{token}/revoke")]
    [Authorize(Policy = "CanManageMembers")]
    [SwaggerOperation(Summary = "Revoga um convite (requer permissão).")]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> RevokeInvite(string token)
    {
        await _service.RevokeInviteAsync(token);

        return Ok(ResultViewModel.Ok("Convite cancelado com sucesso."));
    }
}
