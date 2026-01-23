using archFlowServer.Models.Entities;
using archFlowServer.Utils;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace archFlowServer.Services;

public class TokenService
{
    private readonly SecuritySettings _settings;

    public TokenService(IOptions<SecuritySettings> settings)
    {
        _settings = settings.Value;
    }

    public string GenerateToken(User user)
    {
        var key = Encoding.UTF8.GetBytes(_settings.JwtSecret);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("Name", user.Name),
            new Claim("Email", user.Email),
            new Claim("Type", user.Type.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_settings.JwtExpirationMinutes),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256
            )
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }
}
