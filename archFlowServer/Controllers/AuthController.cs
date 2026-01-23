using archFlowServer.Models.Dtos.Auth;
using archFlowServer.Models.Dtos.User;
using archFlowServer.Models.ViewModels;
using archFlowServer.Services;
using archFlowServer.Utils.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    // ============================
    // LOGIN
    // ============================
    [HttpPost("login")]
    [SwaggerOperation(Summary = "Login do usuário.")]
    [ProducesResponseType(typeof(ResultViewModel<ResponseUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultViewModel), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var (user, token) = await _authService.LoginAsync(dto.Email, dto.Password);

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

        return Ok(
            ResultViewModel<ResponseUserDto>.Ok(
                "Login realizado com sucesso.",
                user
            )
        );
    }

    // ============================
    // LOGOUT
    // ============================
    [Authorize]
    [HttpPost("logout")]
    [SwaggerOperation(Summary = "Logout do usuário.")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("token");

        return Ok(
            ResultViewModel.Ok("Logout realizado com sucesso.")
        );
    }

    // ============================
    // /ME
    // ============================
    [Authorize]
    [HttpGet("me")]
    [SwaggerOperation(Summary = "Retorna os dados do usuário autenticado.")]
    [ProducesResponseType(typeof(ResultViewModel<ResponseUserDto>), StatusCodes.Status200OK)]
    public IActionResult Me()
    {
        var user = _authService.GetAuthenticatedUser(User);

        return Ok(
            ResultViewModel<ResponseUserDto>.Ok(
                "usuário autenticado.",
                user
            )
        );
    }
}

