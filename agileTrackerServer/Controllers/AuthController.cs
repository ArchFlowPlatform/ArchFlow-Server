using agileTrackerServer.Models.Dtos.Auth;
using agileTrackerServer.Models.Dtos.User;
using agileTrackerServer.Models.ViewModels;
using agileTrackerServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using agileTrackerServer.Models.Enums;

namespace agileTrackerServer.Controllers;

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
    [HttpPost("logout")]
    [SwaggerOperation(Summary = "Logout do usuário.")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("token");

        return Ok(ResultViewModel.Ok("Logout realizado com sucesso."));
    }

    // ============================
    // /ME
    // ============================
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(
                ResultViewModel.Fail("Usuário não autenticado.")
            );
        }

        var typeClaim = User.FindFirstValue("Type");

        var userType = Enum.TryParse<UserType>(
            typeClaim,
            ignoreCase: true,
            out var parsedType
        )
            ? parsedType
            : UserType.Free;

        var user = new ResponseUserDto
        {
            Id = Guid.Parse(userId),
            Name = User.FindFirstValue("Name") ?? string.Empty,
            Email = User.FindFirstValue("Email") ?? string.Empty,
            Type = userType
        };

        return Ok(
            ResultViewModel<ResponseUserDto>.Ok(
                "Usuário autenticado.",
                user
            )
        );
    }
}
