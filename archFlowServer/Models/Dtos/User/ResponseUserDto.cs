using archFlowServer.Models.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace archFlowServer.Models.Dtos.User;

[SwaggerSchema(Description = "Dados de retorno de um usuário.")]
public class ResponseUserDto
{
    [SwaggerSchema("Identificador Ãºnico do usuário.")]
    public Guid Id { get; set; }

    [SwaggerSchema("Nome do usuário.")]
    public string Name { get; set; } = string.Empty;

    [SwaggerSchema("Email do usuário.")]
    public string Email { get; set; } = string.Empty;

    [SwaggerSchema("Tipo do usuário, representando sua assinatura.")]
    public UserType Type { get; set; } = UserType.Free;

    [SwaggerSchema("URL do avatar do usuário.")]
    public string? AvatarUrl { get; set; }

    [SwaggerSchema("Data de criaÃ§Ã£o do registro.")]
    public DateTime CreatedAt { get; set; }

    [SwaggerSchema("Data de atualizaÃ§Ã£o do registro.")]
    public DateTime UpdatedAt { get; set; }

    // ============================
    // âœ… CONSTRUTOR DE MAPEAMENTO
    // ============================
    public ResponseUserDto(Entities.User user)
    {
        Id = user.Id;
        Name = user.Name;
        Email = user.Email;
        Type = user.Type;
        AvatarUrl = user.AvatarUrl;
        CreatedAt = user.CreatedAt;
        UpdatedAt = user.UpdatedAt;
    }

    // ============================
    // ðŸ”¹ CONSTRUTOR VAZIO
    // NecessÃ¡rio para serializaÃ§Ã£o / Swagger
    // ============================
    public ResponseUserDto() { }
}
