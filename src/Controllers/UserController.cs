
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;

namespace Zggff.MaiPractice.Controllers;

record LoginResult(string Token);

[Route("user")]
[ApiController]
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
        return Ok(new LoginResult(CreateToken(user)));
    }

    [SwaggerOperation("register")]
    [SwaggerResponse(StatusCodes.Status200OK, "registration successfull", typeof(LoginResult))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "user with login already exists", typeof(void))]
    [HttpPost("register")]
    public async Task<IActionResult> Register([Required][FromBody] User user)
    {
        user.Login = user.Login.ToLower();
        user.Role = UserRole.User;
        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
        user.Id = 0;

        if (await _context.Users.AnyAsync(b => b.Login == user.Login))
        {
            return BadRequest();
        }
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return Ok(new LoginResult(CreateToken(user)));
    }

    [SwaggerOperation("list all users")]
    [SwaggerResponse(StatusCodes.Status200OK, "ok", typeof(User[]))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "user is not authorised", typeof(void))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "user does not have permission to perform action", typeof(void))]
    [HttpGet("list"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> List(UserRole? role)
    {
        if (role == null)
        {
            return Ok(await _context.Users.ToListAsync());
        }
        return Ok(await _context.Users.Where(u => u.Role == role).ToListAsync());
    }

    [SwaggerOperation("update user information. User must be logged in")]
    [SwaggerResponse(StatusCodes.Status200OK, "ok", typeof(LoginResult))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "user is not authorised", typeof(void))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "user does not exist", typeof(void))]
    [HttpPut("update"), Authorize]
    public async Task<IActionResult> Update(User new_user)
    {
        var claimsIdentity = User.Identity as ClaimsIdentity;
        if (claimsIdentity == null)
        {
            return Unauthorized();
        }
        var loginClaim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
        if (loginClaim == null)
        {
            return Unauthorized();
        }
        var login = loginClaim.Value;
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Login == login);
        if (user == null)
        {
            return NotFound("User " + login + " does not exist");
        }

        user.Login = new_user.Login.ToLower();
        user.Password = BCrypt.Net.BCrypt.HashPassword(new_user.Password);

        await _context.SaveChangesAsync();
        // we return new token, as it has the updated login
        return Ok(new LoginResult(CreateToken(user)));
    }

    [SwaggerOperation("elevate user into admin. User must be logged in as Admin")]
    [SwaggerResponse(StatusCodes.Status200OK, "ok")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "user does not exist", typeof(void))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "user is not authorised", typeof(void))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "user does not have permission to perform action", typeof(void))]
    [HttpPut("elevate"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Elevate(uint id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound("user with id " + id + " does not exist");
        }
        user.Role = UserRole.Admin;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [SwaggerResponse(StatusCodes.Status200OK, "ok", typeof(User))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "user does not exist", typeof(void))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "user is not authorised", typeof(void))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "user does not have permission to perform action", typeof(void))]
    [HttpGet("{login}"), Authorize]
    public async Task<IActionResult> GetUser(string? login)
    {
        var claimsIdentity = User.Identity as ClaimsIdentity;
        if (claimsIdentity == null)
        {
            return Unauthorized();
        }
        var loginClaim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
        var roleClaim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);
        if (loginClaim == null || roleClaim == null)
        {
            return Unauthorized();
        }
        if (loginClaim.Value != login && roleClaim.Value != UserRole.Admin.ToString())
        {
            return Forbid();
        }
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Login == login);
        if (user == null)
        {
            return NotFound("User " + login + " does not exist");
        }
        return Ok(user);
    }

    [SwaggerResponse(StatusCodes.Status200OK, "ok", typeof(User))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "user does not exist", typeof(void))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "user is not authorised", typeof(void))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "user does not have permission to perform action", typeof(void))]
    [HttpDelete("{login}"), Authorize]
    public async Task<IActionResult> DeleteUser(string? login)
    {
        var claimsIdentity = User.Identity as ClaimsIdentity;
        if (claimsIdentity == null)
        {
            return Unauthorized();
        }
        var loginClaim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
        if (loginClaim == null)
        {
            return Unauthorized();
        }
        if (loginClaim.Value != login)
        {
            return Forbid();
        }
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Login == login);
        if (user == null)
        {
            return NotFound("User " + login + " does not exist");
        }
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return Ok();
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