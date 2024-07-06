
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;

namespace Zggff.MaiPractice.Controllers;

record LoginResult(string Token);

public class UserController(IConfiguration configuration, AppDbContext context) : ControllerBase
{
    private AppDbContext _context { get; } = context;
    private IConfiguration _configuration { get; } = configuration;


    [SwaggerOperation("login")]
    [SwaggerResponse(StatusCodes.Status200OK, "username and password are correct", typeof(LoginResult))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "incorrect username or password", typeof(void))]
    [HttpGet("login")]
    public async Task<IActionResult> Login([Required] string login, [Required] string password)
    {
        var login_lower = login.ToLower();
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Login == login_lower);
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

    [SwaggerOperation("register")]
    [SwaggerResponse(StatusCodes.Status200OK, "registration successfull", typeof(LoginResult))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "user with login already exists", typeof(void))]
    [HttpPost("register")]
    public async Task<IActionResult> Register([Required] string login, [Required] string password)
    {
        var login_lower = login.ToLower();
        if (await _context.Users.AnyAsync(b => b.Login == login_lower))
        {
            return BadRequest();
        }
        var user = new User
        {
            Login = login_lower,
            Password = BCrypt.Net.BCrypt.HashPassword(password),
            Role = Role.User,
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return Ok(new { Token = CreateToken(user) });
    }

    private string CreateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? ""));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, user.Login != null ? user.Login : ""),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
            ]),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],

        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

}