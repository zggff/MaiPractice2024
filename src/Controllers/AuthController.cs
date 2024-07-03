
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Zggff.MaiPractice.Controllers;

public class AuthController(IConfiguration configuration, AppDbContext context) : ControllerBase
{
    private AppDbContext Context { get; } = context;
    public IConfiguration Configuration { get; } = configuration;


    [HttpPost("login")]
    public async Task<IActionResult> Login(string login, string password)
    {
        var login_lower = login.ToLower();
        var user = await Context.Users.SingleOrDefaultAsync(u => u.Login == login_lower);
        if (user == null)
        {
            return Unauthorized("User " + login_lower + " does not exist");
        }
        if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            return Unauthorized("Invalid username of password");
        }
        return Ok(new { Token = CreateToken(user) });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(string login, string password)
    {
        var login_lower = login.ToLower();
        if (await Context.Users.AnyAsync(b => b.Login == login_lower))
        {
            return BadRequest();
        }
        var user = new User
        {
            Login = login_lower,
            Password = BCrypt.Net.BCrypt.HashPassword(password),
            Role = Role.User,
        };
        await Context.Users.AddAsync(user);
        await Context.SaveChangesAsync();
        return Ok();
    }

    public string CreateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"] ?? ""));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.Name, user.Login != null ? user.Login : ""),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
            Issuer = Configuration["Jwt:Issuer"],
            Audience = Configuration["Jwt:Audience"],

        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

}