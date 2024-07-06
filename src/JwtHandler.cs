using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Zggff.MaiPractice;

public class JwtHandler(IConfiguration configuration, IHttpContextAccessor accessor)
{
    private IConfiguration configuration { get; } = configuration;
    private IHttpContextAccessor accessor { get; } = accessor;

    public string? Claim(string claim_type)
    {
        return accessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == claim_type)?.Value;
    }

    public string Token(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? ""));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, user.Login != null ? user.Login : ""),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
            ]),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"],

        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}