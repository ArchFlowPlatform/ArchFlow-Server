using archFlowServer.Models.Enums;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace archFlowServer.Models.Dtos.User;

[SwaggerSchema(Description = "DTO usado para criar um usuário no sistema.")]
public class CreateUserDto
{
    [SwaggerSchema("Nome completo do usuário.")]
    [Required(ErrorMessage = "Nome é obrigatório.")]
    public string Name { get; set; } = string.Empty;

    [SwaggerSchema("Email do usuário. Deve ser único.")]
    [Required(ErrorMessage = "Email é obrigatório.")]
    public string Email { get; set; } = string.Empty;
    
    [SwaggerSchema("Tipo do usuário (exemplo: 'Free', 'Plus').")]
    public UserType Type { get; set; } = UserType.Free;

    [SwaggerSchema("Senha em texto puro para cadastro. Será hasheada no servidor.")]
    [Required(ErrorMessage = "Senha é obrigatória.")]
    public string Password { get; set; } = string.Empty;
    
    [SwaggerSchema("Url da imagem de perfil do usuário")]
    public string? AvatarUrl { get; set; } = string.Empty;
}

