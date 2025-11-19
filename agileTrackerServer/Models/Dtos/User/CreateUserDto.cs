using agileTrackerServer.Models.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace agileTrackerServer.Models.Dtos.User;

[SwaggerSchema(Description = "DTO usado para criar um usuário no sistema.")]
public class CreateUserDto
{
    [SwaggerSchema("Nome completo do usuário.")]
    public string Name { get; set; } = string.Empty;

    [SwaggerSchema("Email do usuário. Deve ser único.")]
    public string Email { get; set; } = string.Empty;
    
    [SwaggerSchema("Tipo do usuário (exemplo: 'Free', 'Plus').")]
    public UserType Type { get; set; } = UserType.Free;

    [SwaggerSchema("Senha em texto puro para cadastro. Será hasheada no servidor.")]
    public string Password { get; set; } = string.Empty;
    
    [SwaggerSchema("Url da imagem de perfil do usuário")]
    public string? AvatarUrl { get; set; } = string.Empty;
}
