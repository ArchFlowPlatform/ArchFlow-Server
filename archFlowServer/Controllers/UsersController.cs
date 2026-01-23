using archFlowServer.Models.Dtos.User;
using archFlowServer.Models.ViewModels;
using archFlowServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _service;

    public UsersController(UserService service)
    {
        _service = service;
    }

    // ============================
    // GET api/users/{id}
    // ============================
    [Authorize]
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "ObtÃ©m um usuário pelo ID.")]
    [ProducesResponseType(typeof(ResultViewModel<ResponseUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        if (!Guid.TryParse(id, out var userId))
        {
            return BadRequest(
                ResultViewModel.Fail(
                    "ID inválido.",
                    new List<string> { "O ID informado não Ã© um GUID vÃ¡lido." }
                )
            );
        }

        var user = await _service.GetByIdAsync(userId);

        return Ok(
            ResultViewModel<ResponseUserDto>.Ok(
                "usuário encontrado com sucesso.",
                user
            )
        );
    }

    // ============================
    // POST api/users
    // ============================
    [HttpPost]
    [SwaggerOperation(Summary = "Cria um novo usuário.")]
    [ProducesResponseType(typeof(ResultViewModel<ResponseUserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateUserDto request)
    {
        var (user, token) = await _service.CreateAsync(request);

        Response.Cookies.Append(
            "token",
            token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/"
            }
        );

        return CreatedAtAction(
            nameof(GetById),
            new { id = user.Id },
            ResultViewModel<ResponseUserDto>.Ok(
                "usuário criado com sucesso!",
                user
            )
        );
    }
}

