using archFlowServer.Models.Dtos.Project;
using archFlowServer.Models.Enums;
using archFlowServer.Models.ViewModels;
using archFlowServer.Services;
using archFlowServer.Utils.Authorization;
using archFlowServer.Utils.Extensions;
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
    [Authorize]
    [SwaggerOperation(Summary = "Lista todos os projetos ativos do usuário logado.")]
    [ProducesResponseType(typeof(ResultViewModel<IEnumerable<ProjectResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.GetUserId();

        var projects = await _service.GetAllAsync(userId);

        return Ok(
            ResultViewModel<IEnumerable<ProjectResponseDto>>.Ok(
                "Projetos carregados com sucesso.",
                projects
            )
        );
    }

    // GET api/projects/{id}
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

        return Ok(
            ResultViewModel<ProjectResponseDto>.Ok(
                "Projeto encontrado com sucesso.",
                project
            )
        );
    }

    // POST api/projects
    [Authorize]
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
            ResultViewModel<ProjectResponseDto>.Ok(
                "Projeto criado com sucesso.",
                project
            )
        );
    }

    // PUT api/projects/{id}
    [Authorize]
    [AuthorizeProjectRole(MemberRole.Owner)]
    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Atualiza os dados de um projeto.")]
    [ProducesResponseType(typeof(ResultViewModel<ProjectResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectDto dto)
    {
        var userId = User.GetUserId();

        var updated = await _service.UpdateAsync(id, dto, userId);

        return Ok(
            ResultViewModel<ProjectResponseDto>.Ok(
                "Projeto atualizado com sucesso.",
                updated
            )
        );
    }

    // POST api/projects/{id}/archive
    [Authorize]
    [Authorize(Policy = "CanArchiveProject")]
    [HttpPost("{id:guid}/archive")]
    public async Task<IActionResult> Archive(Guid id)
    {
        var userId = User.GetUserId();

        await _service.ArchiveAsync(id, userId);

        return Ok(ResultViewModel.Ok("Projeto arquivado com sucesso."));
    }

    
    [Authorize]
    [Authorize(Policy = "CanManageMembers")]
    [HttpPost("{id:guid}/members")]
    public async Task<IActionResult> AddMember(
        Guid id,
        AddProjectMemberDto dto)
    {
        var userId = User.GetUserId();

        await _service.AddMemberAsync(
            id,
            userId,
            dto.UserId,
            dto.Role
        );

        return Ok(ResultViewModel.Ok("Membro adicionado com sucesso."));
    }

    [Authorize]
    [Authorize(Policy = "CanManageMembers")]
    [HttpDelete("{id:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId)
    {
        var executorId = User.GetUserId();

        await _service.RemoveMemberAsync(id, executorId, userId);

        return Ok(ResultViewModel.Ok("Membro removido com sucesso."));
    }
    
    [Authorize]
    [Authorize(Policy = "CanViewProject")]
    [HttpGet("{id:guid}/members")]
    public async Task<IActionResult> GetMembers(Guid id)
    {
        var userId = User.GetUserId();

        var members = await _service.GetMembersAsync(id, userId);

        return Ok(
            ResultViewModel.Ok(
                "Membros carregados com sucesso.",
                members
            )
        );
    }
    
    [Authorize]
    [Authorize(Policy = "CanManageMembers")]
    [HttpPost("{id:guid}/invites")]
    public async Task<IActionResult> InviteMember(
        Guid id,
        InviteProjectMemberDto dto)
    {
        var userId = User.GetUserId();

        await _service.InviteMemberAsync(
            id,
            userId,
            dto.Email,
            dto.Role
        );

        return Ok(ResultViewModel.Ok("Convite enviado com sucesso."));
    }

    [AllowAnonymous]
    [HttpPost("invites/{token}/accept")]
    public async Task<IActionResult> AcceptInvite(string token)
    {
        var userId = User.GetUserId();

        await _service.AcceptInviteAsync(token, userId);

        return Ok(ResultViewModel.Ok("Convite aceito com sucesso."));
    }


}

