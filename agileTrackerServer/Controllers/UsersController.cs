using agileTrackerServer.Models.Dtos.User;
using agileTrackerServer.Models.ViewModels;
using agileTrackerServer.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace agileTrackerServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _service;

    public UsersController(UserService service)
    {
        _service = service;
    }
    
    // GET api/users/{id}
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obtém um usuário pelo ID.")]
    [ProducesResponseType(typeof(ResultViewModel<ResponseUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        // 1. Validar GUID manualmente (evita 404 automático sem body)
        if (!Guid.TryParse(id, out var guid))
        {
            return BadRequest(
                ResultViewModel.Fail(
                    "O ID informado é inválido.",
                    new List<string> { "O parâmetro não é um GUID válido." }
                )
            );
        }

        // 2. Buscar usuário
        var user = await _service.GetByIdAsync(guid);

        if (user == null)
        {
            return NotFound(
                ResultViewModel.Fail(
                    "Usuário não encontrado!",
                    new List<string> { "Nenhum usuário com esse ID foi localizado." }
                )
            );
        }

        // 3. Retorno normal
        return Ok(
            ResultViewModel<ResponseUserDto>.Ok(
                "Usuário encontrado com sucesso!",
                user
            )
        );
    }

    
    // POST api/users
    [HttpPost]
    [SwaggerOperation(Summary = "Cria um novo usuário.")]
    [ProducesResponseType(typeof(ResultViewModel<ResponseUserDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateUserDto request)
    {
        var user = await _service.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = user.Id },
            ResultViewModel<ResponseUserDto>.Ok("Usuário criado com sucesso!", user)
        );
    }
}
