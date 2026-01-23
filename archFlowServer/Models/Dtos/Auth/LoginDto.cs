using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace archFlowServer.Models.Dtos.Auth;

public class LoginDto
{
    [SwaggerSchema("Email do usuário")]
    [Required(ErrorMessage = "Email é obrigatório.")]
    public string Email { get; set; } = string.Empty;

    [SwaggerSchema("Senha do usuário")]
    [Required(ErrorMessage = "Senha é obrigatório.")]
    public string Password { get; set; } = string.Empty;
}
