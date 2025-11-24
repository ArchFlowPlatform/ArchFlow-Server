using agileTrackerServer.Models.Dtos.Project;
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
    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Obtém um usuário pelo ID.")]
    [ProducesResponseType(typeof(ResultViewModel<ResponseUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel<ResponseUserDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _service.GetByIdAsync(id);

        if (user == null)
        {
            return Ok(
                ResultViewModel<ResponseUserDto>.Fail(
                    "Usuário não encontrado!",
                    new List<string> { "Nenhum usuário com esse ID foi localizado." }
                )
            );
        }

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
    [ProducesResponseType(typeof(ResponseUserDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ResponseUserDto>> Create([FromBody] CreateUserDto request)
    {
        var user = await _service.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = user.Id },
            ResultViewModel<ResponseUserDto>.Ok("Usuário criado com sucesso!", user)
        );
    }

}