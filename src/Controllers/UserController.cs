
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Zggff.MaiPractice.Components;
using Zggff.MaiPractice.Models;


namespace Zggff.MaiPractice.Controllers;

record LoginResult(string Token);

[Route("user")]
[ApiController]
public class UserController(IConfiguration configuration, AppDbContext context, IHttpContextAccessor accessor) : ControllerBase
{
    private AppDbContext context { get; } = context;
    private ClaimHandler claims { get; } = new ClaimHandler(accessor);
    private JwtHandler jwt { get; } = new JwtHandler(configuration);


    [SwaggerOperation("login")]
    [SwaggerResponse(StatusCodes.Status200OK, "username and password are correct", typeof(LoginResult))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "incorrect username or password", typeof(void))]
    [HttpGet("login")]
    public async Task<IActionResult> Login([Required] string login, [Required] string password)
    {
        var login_lower = login.ToLower();
        var user = await context.Users.SingleOrDefaultAsync(u => u.Login == login_lower);
        if (user == null)
            return Unauthorized("User " + login_lower + " does not exist");

        if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            return Unauthorized("Invalid username of password");
        return Ok(new LoginResult(jwt.Token(user)));
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

        if (await context.Users.AnyAsync(b => b.Login == user.Login))
            return BadRequest();

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        return Ok(new LoginResult(jwt.Token(user)));
    }

    [SwaggerOperation("list all users")]
    [SwaggerResponse(StatusCodes.Status200OK, "ok", typeof(User[]))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "user is not authorised", typeof(void))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "user does not have permission to perform action", typeof(void))]
    [HttpGet("list"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> List(UserRole? role)
    {
        if (role == null)
            return Ok(await context.Users.ToListAsync());

        return Ok(await context.Users.Where(u => u.Role == role).ToListAsync());
    }

    [SwaggerOperation("update user information. User must be logged in")]
    [SwaggerResponse(StatusCodes.Status200OK, "ok", typeof(LoginResult))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "user is not authorised", typeof(void))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "user does not exist", typeof(void))]
    [HttpPut("update"), Authorize]
    public async Task<IActionResult> Update(User new_user)
    {
        var loginClaim = claims.Login();

        var user = await context.Users.SingleOrDefaultAsync(u => u.Login == loginClaim);
        if (user == null)
            return NotFound("User " + loginClaim + " does not exist");

        user.Login = new_user.Login.ToLower();
        user.Password = BCrypt.Net.BCrypt.HashPassword(new_user.Password);

        await context.SaveChangesAsync();
        // we return new token, as it has the updated login
        return Ok(new LoginResult(jwt.Token(user)));
    }

    [SwaggerOperation("elevate user into admin. User must be logged in as Admin")]
    [SwaggerResponse(StatusCodes.Status200OK, "ok")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "user does not exist", typeof(void))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "user is not authorised", typeof(void))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "user does not have permission to perform action", typeof(void))]
    [HttpPut("elevate"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Elevate(uint id)
    {
        var user = await context.Users.FindAsync(id);
        if (user == null)
            return NotFound("user with id " + id + " does not exist");
        user.Role = UserRole.Admin;
        await context.SaveChangesAsync();
        return Ok();
    }

    [SwaggerResponse(StatusCodes.Status200OK, "ok", typeof(User))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "user does not exist", typeof(void))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "user is not authorised", typeof(void))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "user does not have permission to perform action", typeof(void))]
    [HttpGet("{login}"), Authorize]
    public async Task<IActionResult> GetUser(string? login)
    {
        var login_claim = claims.Login();
        var role_claim = claims.Role();
        if (login_claim != login && role_claim != UserRole.Admin)
            return Forbid();

        var user = await context.Users.SingleOrDefaultAsync(u => u.Login == login);
        if (user == null)
            return NotFound("User " + login + " does not exist");
        return Ok(user);
    }

    [SwaggerResponse(StatusCodes.Status200OK, "ok", typeof(User))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "user does not exist", typeof(void))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "user is not authorised", typeof(void))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "user does not have permission to perform action", typeof(void))]
    [HttpDelete("{login}"), Authorize]
    public async Task<IActionResult> DeleteUser(string? login)
    {
        var loginClaim = claims.Login();
        if (loginClaim != login)
            return Forbid();

        var user = await context.Users.SingleOrDefaultAsync(u => u.Login == login);
        if (user == null)
            return NotFound("User " + login + " does not exist");

        context.Users.Remove(user);
        await context.SaveChangesAsync();
        return Ok();
    }
}